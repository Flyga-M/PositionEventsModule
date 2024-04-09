using Blish_HUD.Modules;
using PositionEvents.Area;

namespace Flyga.PositionEventsModule.Contexts
{
    public struct RemoveArea
    {
        public RemoveArea(Module caller, int mapId, IBoundingObject area)
        {
            Caller = caller;
            MapId = mapId;
            Area = area;
        }

        public Module Caller {  get; set; }
        public int MapId { get; set; }
        public IBoundingObject Area { get; set; }
    }
}
