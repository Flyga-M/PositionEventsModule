using Blish_HUD;
using Blish_HUD.Contexts;
using Blish_HUD.Modules;
using PositionEvents;
using PositionEvents.Area;
using System;
using System.Threading.Tasks;

namespace Flyga.PositionEventsModule.Contexts
{
    // 100% inspired by / copied from
    // https://github.com/Tharylia/Blish-HUD-Modules/blob/main/Estreya.BlishHUD.EventTable/Contexts/EventTableContext.cs
    public class PositionEventsContext : Context
    {
        private Logger Logger = Logger.GetLogger<PositionEventsContext>();

        internal delegate Task AsyncEventHandler<TEventArgs>(object sender, TEventArgs e);
        internal delegate Task<TReturn> AsyncReturnEventHandler<TEventArgs, TReturn>(object sender, TEventArgs e);


        internal event AsyncEventHandler<RegisterArea> RequestRegisterArea;
        internal event AsyncReturnEventHandler<RemoveArea, bool> RequestRemoveArea;
        internal event AsyncEventHandler<RemoveAllAreas> RequestRemoveAllAreas;

        protected override void Load()
        {
            this.ConfirmReady();
        }

        /// <summary>
        /// Checks if the context is ready. 
        /// https://github.com/Tharylia/Blish-HUD-Modules/blob/main/Estreya.BlishHUD.Shared/Contexts/BaseContext.cs
        /// </summary>
        /// <exception cref="InvalidOperationException">Throws if context is expired or not ready.</exception>
        protected void CheckReady()
        {
            if (this.State == ContextState.Expired) throw new InvalidOperationException("Context has expired.");
            if (this.State != ContextState.Ready) throw new InvalidOperationException("Context is not ready.");
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
        public async Task RegisterArea(Module module, int mapId, IBoundingObject area, Action<PositionData, bool> callback, bool debug = false)
        {
            CheckReady();

            await (RequestRegisterArea?.Invoke(this, new RegisterArea(module, mapId, area, callback, debug)) ?? Task.FromException(new NotImplementedException()));
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
        public async Task<bool> RemoveArea(Module module, int mapId, IBoundingObject area)
        {
            CheckReady();

            if (RequestRemoveArea is null) throw new NotImplementedException();

            return await RequestRemoveArea.Invoke(this, new RemoveArea(module, mapId, area));
        }

        /// <summary>
        /// Removes all <see cref="IBoundingObject">areas</see> that were registered for the given 
        /// <paramref name="module"/>.
        /// </summary>
        /// <param name="module">The <see cref="Module"/> that registered the areas.</param>
        public async Task RemoveAllAreas(Module module)
        {
            CheckReady();

            await (RequestRemoveAllAreas?.Invoke(this, new RemoveAllAreas(module)) ?? Task.FromException(new NotImplementedException()));
        }

    }
}
