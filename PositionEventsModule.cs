using Blish_HUD;
using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Settings;
using Flyga.PositionEventsModule.Contexts;
using Microsoft.Xna.Framework;
using PositionEvents;
using PositionEvents.Area;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace Flyga.PositionEventsModule
{
    [Export(typeof(Module))]
    public class PositionEventsModule : Module
    {
        private static readonly Logger Logger = Logger.GetLogger<PositionEventsModule>();

        internal static PositionEventsModule Instance { get; set; }

        private readonly Dictionary<int, List<IBoundingObject>> _debugAreas;

        private readonly Dictionary<Type, Dictionary<int, List<IBoundingObject>>> _areasByModule;

        private readonly List<Module> _registeredModules;

        private IPositionHandler _positionHandler;

        private static SettingEntry<int> _updateCooldown;
        private static SettingEntry<bool> _updateCooldownOverrideAllowed;

        private static int _actualCooldown = 0;

        private Dictionary<Type, int> _cooldownOverridesByModule;

        private double _lastUpdate = 0;

        private PositionEventsContext _positionEventsContext;
        private ContextsService.ContextHandle<PositionEventsContext> _positionEventsContextHandle;
        private ContextManager _contextManager;

        /// <summary>
        /// The value of the UpdateCooldown (refresh rate) setting of this 
        /// <see cref="PositionEventsModule"/>. Clamped between 0 and 5000 ms. 
        /// Will return 0, if the setting is null.
        /// </summary>
        public static int ClampedCooldown
        {
            get
            {
                if (_updateCooldown.IsNull)
                {
                    return 0;
                }
                if (_updateCooldown.Value <= 0)
                {
                    return 0;
                }
                if (_updateCooldown.Value >= 5000)
                {
                    return 5000;
                }

                return _updateCooldown.Value;
            }
        }

        /// <summary>
        /// The actual update cooldown (refresh rate) determined by the UpdateCooldown 
        /// setting, the UpdateCooldownOverrideAllowed setting of this 
        /// <see cref="PositionEventsModule"/> and the registered 
        /// cooldown overrides by the dependent <see cref="Module">Modules</see>.
        /// </summary>
        public static int ActualCooldown
        {
            get
            {
                return _actualCooldown;
            }
        }

        public static bool UpdateCooldownOverrideAllowed
        {
            get
            {
                if (_updateCooldownOverrideAllowed.IsNull)
                {
                    return true;
                }
                return _updateCooldownOverrideAllowed.Value;
            }
        }

        #region Service Managers
        internal SettingsManager SettingsManager => this.ModuleParameters.SettingsManager;
        internal ContentsManager ContentsManager => this.ModuleParameters.ContentsManager;
        internal DirectoriesManager DirectoriesManager => this.ModuleParameters.DirectoriesManager;
        internal Gw2ApiManager Gw2ApiManager => this.ModuleParameters.Gw2ApiManager;
        #endregion

        [ImportingConstructor]
        public PositionEventsModule([Import("ModuleParameters")] ModuleParameters moduleParameters) : base(moduleParameters)
        {
            Instance = this;

            _debugAreas = new Dictionary<int, List<IBoundingObject>>();
            _registeredModules = new List<Module>();
            _areasByModule = new Dictionary<Type, Dictionary<int, List<IBoundingObject>>>();
            _cooldownOverridesByModule = new Dictionary<Type, int>();
        }

        protected override void DefineSettings(SettingCollection settings)
        {
            _updateCooldown = settings.DefineSetting("UpdateCooldown", 30, () => "Update Cooldown (ms)", () => "The cooldown between checking the player position in miliseconds (1000ms = 1s). Clamped between 0s - 5s.");
            _updateCooldown.SetRange(0, 5000);

            _updateCooldownOverrideAllowed = settings.DefineSetting("UpdateCooldownOverrideAllowed", true, () => "Allow modules to ignore cooldown", () => "Allows individual modules to update the position data more often than you specified in the \"Update Cooldown\" setting.");
        }

        protected override void Initialize()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged += OnMapChange;
            _positionHandler = new PositionHandler(MapChanged);

            _updateCooldown.SettingChanged += OnUpdateCooldownSettingChanged;
            _updateCooldownOverrideAllowed.SettingChanged += OnUpdateCooldownOverrideAllowedSettingChanged;

            UpdateActualCooldown();
        }

        private void OnUpdateCooldownSettingChanged(object _, ValueChangedEventArgs<int> _1)
        {
            UpdateActualCooldown();
        }

        private void OnUpdateCooldownOverrideAllowedSettingChanged(object _, ValueChangedEventArgs<bool> _1)
        {
            UpdateActualCooldown();
        }

        private void UpdateActualCooldown()
        {
            int min = ClampedCooldown;

            if (!_updateCooldownOverrideAllowed.IsNull && !_updateCooldownOverrideAllowed.Value)
            {
                _actualCooldown = min;
                return;
            }

            foreach (int overrideCooldown in _cooldownOverridesByModule.Values)
            {
                if (overrideCooldown < min)
                {
                    min = overrideCooldown;
                }
            }

            _actualCooldown = min;
        }

        private event EventHandler<PositionData> MapChanged;

        private void OnMapChange(object _, ValueEventArgs<int> mapId)
        {
            MapChanged?.Invoke(this, GameService.Gw2Mumble.GetPositionData(mapId.Value));

            Debug.BoundingObjectDebug.RemoveAllBoundingObjects();

            LoadCurrentDebugEntities(mapId.Value);
        }

        private void LoadCurrentDebugEntities(int mapId)
        {
            if (!_debugAreas.ContainsKey(mapId))
            {
                return;
            }

            foreach (IBoundingObject area in _debugAreas[mapId])
            {
                Debug.BoundingObjectDebug.DisplayBoundingObject(area);
            }
        }

        protected override Task LoadAsync()
        {
            return Task.CompletedTask;
        }

        private void OnDebugAreaJoinOrLeave(IBoundingObject area, bool joined)
        {
            if (joined)
            {
                Debug.BoundingObjectDebug.ChangeBoundingObject(area, Debug.DebugColor.Green);
                return;
            }

            Debug.BoundingObjectDebug.ChangeBoundingObject(area, Debug.DebugColor.Red);
        }

        private void OnOtherModuleRunStateChanged(object sender, ModuleRunStateChangedEventArgs runStateChangedEventArgs)
        {
            ModuleRunState runState = runStateChangedEventArgs.RunState;

            if (!(sender is Module module))
            {
                return;
            }

            if (!_registeredModules.Contains(module))
            {
                return;
            }

            if (runState != ModuleRunState.Unloaded && runState != ModuleRunState.FatalError)
            {
                return;
            }

            OnOtherModuleUnloaded(module);
            _registeredModules.Remove(module); // unregister module
            _areasByModule.Remove(module.GetType()); // remove artifacts of the module
        }

        private void OnOtherModuleUnloaded(Module module)
        {
            RemoveAllAreas(module);
        }

        protected override void OnModuleLoaded(EventArgs e)
        {
            RegisterContext();

            // Base handler must be called
            base.OnModuleLoaded(e);
        }

        private void RegisterContext()
        {
            _positionEventsContext = new PositionEventsContext();
            _contextManager = new ContextManager(_positionEventsContext, this);
            _positionEventsContextHandle = GameService.Contexts.RegisterContext(_positionEventsContext);
        }

        protected override void Update(GameTime gameTime)
        {
            if (_lastUpdate < ActualCooldown)
            {
                _lastUpdate += gameTime.ElapsedGameTime.TotalMilliseconds;
                return;
            }

            _lastUpdate = 0;

            _positionHandler.Update(GameService.Gw2Mumble.GetPositionData());
        }

        /// <summary>
        /// Registers an <paramref name="area"/> for the map with the given <paramref name="mapId"/>. The 
        /// <paramref name="callback"/> will be invoked once, when the player joins the <paramref name="area"/> 
        /// and once, when the player leaves the <paramref name="area"/>. 
        /// </summary>
        /// <param name="module">The <see cref="Module"/> that registers the area.</param>
        /// <param name="mapId">The mapId in which the <paramref name="area"/> should be active.</param>
        /// <param name="area">The <see cref="IBoundingObject"/> that defines the area.</param>
        /// <param name="callback">The <see cref="Action"/> to be called, when the player leaves or 
        /// joins the <paramref name="area"/>.</param>
        /// <param name="debug">A debug flag. If set to true, the <paramref name="area"/> will be 
        /// rendered visually when in the given map. Should always be set to false when shipping 
        /// a <see cref="Module"/>.</param>
        internal void RegisterArea(Module module, int mapId, IBoundingObject area, Action<PositionData, bool> callback, bool debug = false)
        {
            Type moduleType = module.GetType();
            
            if (!_areasByModule.ContainsKey(moduleType))
            {
                _areasByModule[moduleType] = new Dictionary<int, List<IBoundingObject>>();
            }

            if (!_registeredModules.Contains(module))
            {
                _registeredModules.Add(module);
                module.ModuleRunStateChanged += OnOtherModuleRunStateChanged;
            }

            if (!_areasByModule[moduleType].ContainsKey(mapId))
            {
                _areasByModule[moduleType][mapId] = new List<IBoundingObject>();
            }

            _areasByModule[moduleType][mapId].Add(area);

            
            if (debug == false)
            {
                _positionHandler.AddArea(mapId, area, callback);
                return;
            }

            _positionHandler.AddArea(mapId, area,
                (positionData, isContained) =>
                {
                    OnDebugAreaJoinOrLeave(area, isContained);
                    callback(positionData, isContained);
                });

            if (!_debugAreas.ContainsKey(mapId))
            {
                _debugAreas[mapId] = new List<IBoundingObject>();
            }

            _debugAreas[mapId].Add(area);

            if (mapId == GameService.Gw2Mumble.CurrentMap.Id)
            {
                Debug.BoundingObjectDebug.DisplayBoundingObject(area);
            }
        }

        private bool RemoveArea(Type moduleType, int mapId, IBoundingObject area)
        {
            if (_debugAreas.ContainsKey(mapId) && _debugAreas[mapId].Contains(area))
            {
                _debugAreas[mapId].Remove(area);
                Debug.BoundingObjectDebug.RemoveBoundingObject(area);
            }

            if (_areasByModule.ContainsKey(moduleType)
                && _areasByModule[moduleType].ContainsKey(mapId)
                && _areasByModule[moduleType][mapId].Contains(area))
            {
                _areasByModule[moduleType][mapId].Remove(area);
            }

            return _positionHandler.RemoveArea(mapId, area);
        }

        /// <summary>
        /// Removes an <paramref name="area"/> for the map with the given <paramref name="mapId"/>, 
        /// that was registered via 
        /// <see cref="RegisterArea(Module, int, IBoundingObject, Action{PositionData, bool}, bool)"/>.
        /// </summary>
        /// <param name="module">The <see cref="Module"/> that registered the area.</param>
        /// <param name="mapId">The mapId for which the <paramref name="area"/> was registered.</param>
        /// <param name="area">The <see cref="IBoundingObject"/> that defined the area.</param>
        /// <returns>True, if the <paramref name="area"/> was registered for the given <paramref name="mapId"/> 
        /// and successfully removed. Otherwise false.</returns>
        internal bool RemoveArea(Module module, int mapId, IBoundingObject area)
        {
            return RemoveArea(module.GetType(), mapId, area);
        }

        /// <summary>
        /// Removes all <see cref="IBoundingObject">areas</see> that were registered for the given 
        /// <paramref name="module"/>.
        /// </summary>
        /// <param name="module">The <see cref="Module"/> that registered the areas.</param>
        internal void RemoveAllAreas(Module module)
        {
            Type moduleType = module.GetType();

            if (!_areasByModule.ContainsKey(moduleType))
            {
                return;
            }

            Dictionary<int, List<IBoundingObject>> areasForModule = _areasByModule[moduleType];

            foreach (KeyValuePair<int, List<IBoundingObject>> areasByMapId in areasForModule)
            {
                int mapId = areasByMapId.Key;
                IBoundingObject[] areas = areasByMapId.Value.ToArray(); // copy values, so we can remove them while iterating
                foreach (IBoundingObject area in areas)
                {
                    RemoveArea(moduleType, mapId, area);
                }
            }
        }

        /// <summary>
        /// Registers the cooldown (refresh rate) override <paramref name="value"/> for 
        /// the given <paramref name="module"/>.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="value"></param>
        /// <returns>The current <see cref="PositionEventsModule.ActualCooldown"/> 
        /// value.</returns>
        /// <exception cref="ArgumentException">If the set value is less than 0.</exception>
        internal int OverrideCooldown(Module module, int value)
        {
            if (value < 0)
            {
                throw new ArgumentException("value can't be less than 0.", nameof(value));
            }

            _cooldownOverridesByModule[module.GetType()] = value;

            UpdateActualCooldown();

            return ActualCooldown;
        }

        private void UnloadContext()
        {
            _positionEventsContextHandle?.Expire();

            if (this._contextManager != null)
            {
                this._contextManager.Dispose();
                this._contextManager = null;
            }

            this._positionEventsContext = null;
            this._positionEventsContextHandle = null;
        }

        /// <inheritdoc />
        protected override void Unload()
        {
            GameService.Gw2Mumble.CurrentMap.MapChanged -= OnMapChange;

            foreach (Module module in _registeredModules)
            {
                module.ModuleRunStateChanged -= OnOtherModuleRunStateChanged;
            }

            UnloadContext();

            _registeredModules.Clear();
            _areasByModule.Clear();
            _cooldownOverridesByModule.Clear();

            _positionHandler.Clear();
            _positionHandler.Dispose();

            _updateCooldown.SettingChanged -= OnUpdateCooldownSettingChanged;
            _updateCooldownOverrideAllowed.SettingChanged -= OnUpdateCooldownOverrideAllowedSettingChanged;

            Debug.BoundingObjectDebug.RemoveAllBoundingObjects();
        }

    }

}
