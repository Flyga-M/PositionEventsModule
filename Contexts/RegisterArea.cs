using Blish_HUD.Modules;
using PositionEvents;
using PositionEvents.Area;
using System;

namespace Flyga.PositionEventsModule.Contexts
{
    public struct RegisterArea
    {
        public RegisterArea(Module caller, int mapId, IBoundingObject area, Action<PositionData, bool> callback, bool debug)
        {
            Caller = caller;
            MapId = mapId;
            Area = area;
            Callback = callback;
            Debug = debug;
        }

        public Module Caller { get; set; }
        public int MapId { get; set; }
        public IBoundingObject Area { get; set; }
        public Action<PositionData, bool> Callback { get; set; }
        public bool Debug { get; set; }

    }
}
