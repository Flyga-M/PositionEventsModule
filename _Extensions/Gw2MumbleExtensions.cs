using Blish_HUD;
using PositionEvents;

namespace Flyga.PositionEventsModule
{
    public static class Gw2MumbleExtensions
    {
        /// <summary>
        /// Returns the current <see cref="PositionData"/> of the player. If a <paramref name="mapId"/> other than 
        /// null is given, it will override the <see cref="PositionData.MapId"/> component.
        /// </summary>
        /// <param name="mumble">The instance of the <see cref="Gw2MumbleService"/>.</param>
        /// <param name="mapId">An optional <see cref="PositionData.MapId"/> override.</param>
        /// <returns>The current <see cref="PositionData"/> of the player.</returns>
        public static PositionData GetPositionData(this Gw2MumbleService mumble, int? mapId = null)
        {
            PositionData positionData = new PositionData()
            {
                MapId = mapId ?? mumble.CurrentMap.Id,
                Position = mumble.PlayerCharacter.Position
            };

            return positionData;
        }
    }
}
