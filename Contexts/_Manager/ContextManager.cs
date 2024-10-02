using Blish_HUD;
using Microsoft.Xna.Framework;
using System;
using System.Threading.Tasks;

namespace Flyga.PositionEventsModule.Contexts
{
    // 100% inspired by / copied from
    // https://github.com/Tharylia/Blish-HUD-Modules/blob/main/Estreya.BlishHUD.EventTable/Managers/ContextManager.cs
    class ContextManager : IDisposable, IUpdatable
    {
        private static Logger Logger = Logger.GetLogger<ContextManager>();

        private PositionEventsContext _context;
        private PositionEventsModule _module;

        public ContextManager(PositionEventsContext context, PositionEventsModule module)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (module == null)
            {
                throw new ArgumentNullException(nameof(module));
            }

            _context = context;
            _module = module;

            _context.RequestRegisterArea += RequestAddArea;
            _context.RequestRemoveArea += RequestRemoveArea;
            _context.RequestRemoveAllAreas += RequestRemoveAllAreas;
            _context.RequestOverrideCooldown += RequestOverrideCooldown;
        }

        private Task RequestAddArea(object _, RegisterArea arguments)
        {
            _module.RegisterArea(arguments.Caller, arguments.MapId, arguments.Area, arguments.Callback, arguments.Debug);

            return Task.CompletedTask;
        }

        private Task<bool> RequestRemoveArea(object _, RemoveArea arguments)
        {
            bool removeEval = _module.RemoveArea(arguments.Caller, arguments.MapId, arguments.Area);

            return Task.FromResult(removeEval);
        }

        private Task RequestRemoveAllAreas(object _, RemoveAllAreas arguments)
        {
            _module.RemoveAllAreas(arguments.Caller);

            return Task.CompletedTask;
        }

        private Task<int> RequestOverrideCooldown(object _, OverrideCooldown arguments)
        {
            int actualCooldown = _module.OverrideCooldown(arguments.Caller, arguments.Value);

            return Task.FromResult(actualCooldown);
        }

        public void Update(GameTime gameTime)
        {
            /** NOOP **/
        }

        public void Dispose()
        {
            if (_context != null)
            {
                _context.RequestRegisterArea -= RequestAddArea;
                _context.RequestRemoveArea -= RequestRemoveArea;
                _context.RequestRemoveAllAreas -= RequestRemoveAllAreas;
                _context.RequestOverrideCooldown -= RequestOverrideCooldown;
            }

            _context = null;
            _module = null;
        }
    }
}
