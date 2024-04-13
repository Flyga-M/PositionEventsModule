using Blish_HUD.Modules;

namespace Flyga.PositionEventsModule.Contexts
{
    public struct OverrideCooldown
    {
        public OverrideCooldown(Module caller, int value)
        {
            Caller = caller;
            Value = value;
        }
        
        public Module Caller { get; set; }
        public int Value { get; set; }
    }
}
