using System;
using UnityEngine;

namespace Vchod.Systems
{
    /// <summary>CO model tuning (GDD: value 0–100, thresholds 20/40/60/80/100). All rates are units per second.</summary>
    [Serializable]
    public sealed class CoSettings
    {
        [Range(0f, 100f)] public float startLevel = 5f;
        [Tooltip("Within this distance of the leaking flat (m) CO rises faster, steeply near it.")]
        public float sourceRadius = 10f;
        [Tooltip("Rise at the source itself. 2.5/s = about 40 s from nothing to collapse.")]
        public float sourceMaxRate = 2.5f;
        [Tooltip("Slow rise anywhere in the corridors (gas spreads through the shafts).")]
        public float corridorRate = 0.08f;
        [Tooltip("Stairwell: slowly clears (negative = falls).")]
        public float stairwellRate = -0.3f;
        [Tooltip("At an open window: clears fast.")]
        public float windowRate = -4f;
        [Tooltip("Stage boundaries. The last one is collapse.")]
        public float[] thresholds = { 20f, 40f, 60f, 80f, 100f };
    }

    /// <summary>Where the player is breathing, as the CO model needs it.</summary>
    public struct CoExposure
    {
        public float DistanceToSource;
        public bool InStairwell;
        public bool AtOpenWindow;
    }

    [Serializable]
    public sealed class DetectorSettings
    {
        [Tooltip("Beep interval right at the source (s).")]
        public float minInterval = 0.3f;
        [Tooltip("Beep interval at the edge of hearing (s).")]
        public float maxInterval = 2.2f;
        [Tooltip("Beyond this distance (m) the interval stops growing; 3D rolloff fades the volume.")]
        public float range = 24f;
        public float volume = 0.7f;
        public float frequency = 3100f;
        public float beepLength = 0.09f;

        /// <summary>The fair orientation cue: faster beeping = closer to the leak.</summary>
        public float IntervalFor(float distance) =>
            Mathf.Lerp(minInterval, maxInterval, Mathf.Clamp01(distance / range));
    }
}
