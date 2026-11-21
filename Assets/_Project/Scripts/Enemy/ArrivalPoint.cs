using System.Collections.Generic;
using UnityEngine;

namespace Vchod.Enemy
{
    /// <summary>Where the firefighter walks onto a floor: the entry farthest from the player, never too close.</summary>
    public static class ArrivalPoint
    {
        public static bool Pick(IReadOnlyList<Vector3> entries, Vector3 player, float minDistance, out Vector3 point)
        {
            point = default;
            var best = -1f;
            foreach (var e in entries)
            {
                var d = Vector3.Distance(e, player);
                if (d < minDistance || d <= best) continue;
                best = d;
                point = e;
            }
            return best >= 0f;
        }
    }
}
