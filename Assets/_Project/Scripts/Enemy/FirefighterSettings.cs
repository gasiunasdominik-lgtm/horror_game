using System;
using UnityEngine;

namespace Vchod.Enemy
{
    public enum FirefighterState
    {
        Patrol = 0,
        Investigate = 1,
        Chase = 2,
        Call = 3,
        Search = 4,
    }

    /// <summary>Firefighter tuning. He is slower than a sprint: escape is possible, but noisy (GDD).</summary>
    [Serializable]
    public sealed class FirefighterSettings
    {
        [Header("Speed (m/s)")]
        public float patrolSpeed = 1.2f;
        public float investigateSpeed = 1.7f;
        public float chaseSpeed = 3.0f;
        public float searchSpeed = 1.5f;

        [Header("Sight")]
        [Tooltip("Sight range with the staircase light on (m).")]
        public float sightRangeLit = 14f;
        [Range(10f, 180f)] public float sightAngle = 110f;
        [Tooltip("In the dark he sees only what his torch lights: its range (m)...")]
        public float sightRangeDark = 9f;
        [Tooltip("...and its cone (deg). The visible beam tells the player exactly where he looks.")]
        [Range(10f, 120f)] public float torchAngle = 50f;
        [Tooltip("Within this distance he notices the player in any direction (breathing, rustle).")]
        public float nearSenseRadius = 2f;
        [Tooltip("How fast he turns while looking around (deg/s).")]
        public float lookAroundSpeed = 70f;

        [Header("Timing (s)")]
        [Tooltip("Lost sight this long during a chase → he stops and calls.")]
        public float loseSightTime = 2f;
        public float callDuration = 3.5f;
        public float searchDuration = 18f;
        [Tooltip("Looking around after reaching a noise or light.")]
        public float investigateLookTime = 4f;

        [Header("Contact")]
        [Tooltip("This close during a chase = he has you (evacuation, not violence).")]
        public float catchRadius = 1.1f;
        [Tooltip("Search points are picked within this many metres of the last known position.")]
        public float searchRadius = 8f;

        /// <summary>Sight range and cone for the current staircase light.</summary>
        public void SightFor(bool lightOn, out float range, out float angle)
        {
            range = lightOn ? sightRangeLit : sightRangeDark;
            angle = lightOn ? sightAngle : torchAngle;
        }
    }
}
