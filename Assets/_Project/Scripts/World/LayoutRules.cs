using System;
using UnityEngine;

namespace Vchod.World
{
    /// <summary>Tunable shape of a generated floor. Lives inside HouseConfig.</summary>
    [Serializable]
    public sealed class LayoutRules
    {
        [Tooltip("Corridor cells from the stairwell to the player's door (stairwell excluded).")]
        [Min(2)] public int mainPathMin = 5;
        [Min(2)] public int mainPathMax = 7;
        [Tooltip("Side corridors branching off the main path.")]
        [Min(0)] public int branchCount = 2;
        [Min(1)] public int branchMaxLength = 2;
        [Tooltip("Max cells from the stairwell in any axis.")]
        [Min(2)] public int gridRadius = 5;
        [Range(0f, 1f)] public float apartmentDoorChance = 0.55f;
        [Min(1)] public int minApartmentDoors = 2;
        [Min(1)] public int maxAttempts = 50;
    }
}
