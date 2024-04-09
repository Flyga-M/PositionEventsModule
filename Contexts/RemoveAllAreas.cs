using Blish_HUD.Modules;

namespace Flyga.PositionEventsModule.Contexts
{
    public struct RemoveAllAreas
    {
        public RemoveAllAreas(Module caller)
        {
            Caller = caller;
        }

        public Module Caller {  get; set; }
    }
}
