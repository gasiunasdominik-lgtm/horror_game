using System.Collections.Generic;
using UnityEngine;

namespace Vchod.Enemy
{
    /// <summary>Which hiding spot the firefighter checks next. Pure, so the rule is testable.</summary>
    public static class HidingSearch
    {
        /// <param name="spots">Check points of all hiding spots on the floor.</param>
        /// <param name="sawHide">Index of the spot he saw the player enter, or -1: always checked first.</param>
        /// <param name="roll">Uniform 0–1; below <paramref name="chance"/> he checks the nearest unchecked spot.</param>
        /// <param name="considered">The nearest unchecked spot in range, decided either way. The caller
        /// marks it as done, so each spot gets one roll per search (he checks it or walks past).</param>
        /// <returns>Spot index, or -1 to keep wandering.</returns>
        public static int Pick(IReadOnlyList<Vector3> spots, Vector3 center, float radius,
            ICollection<int> alreadyChecked, int sawHide, double roll, float chance, out int considered)
        {
            considered = -1;
            if (sawHide >= 0 && sawHide < spots.Count && !alreadyChecked.Contains(sawHide))
            {
                considered = sawHide;
                return sawHide;
            }

            var best = -1;
            var bestSq = radius * radius;
            for (var i = 0; i < spots.Count; i++)
            {
                if (alreadyChecked.Contains(i)) continue;
                var d = spots[i] - center;
                d.y = 0f;
                if (d.sqrMagnitude > bestSq) continue;
                bestSq = d.sqrMagnitude;
                best = i;
            }
            considered = best;
            return roll < chance ? best : -1;
        }
    }
}
