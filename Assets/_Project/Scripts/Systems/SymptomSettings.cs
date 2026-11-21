using System;
using UnityEngine;

namespace Vchod.Systems
{
    /// <summary>
    /// How CO shows on the body (GDD pillar 1: symptoms are true information).
    /// Each effect ramps in over <see cref="ramp"/> units and is full at its threshold.
    /// </summary>
    [Serializable]
    public sealed class SymptomSettings
    {
        public float ramp = 10f;

        [Header("20: ringing in the ears")]
        public float humThreshold = 20f;
        [Range(0f, 1f)] public float humMaxVolume = 0.22f;

        [Header("40: narrowed view, slower turning")]
        public float narrowThreshold = 40f;
        [Range(0f, 1f)] public float baseVignette = 0.3f;
        [Range(0f, 1f)] public float narrowVignette = 0.48f;
        [Range(0f, 1f)] public float narrowLookMultiplier = 0.7f;

        [Header("60: warped geometry, muffled sound")]
        public float warpThreshold = 60f;
        [Range(0f, 1f)] public float warpDistortion = 0.35f;
        public float warpSpeed = 0.7f;
        [Range(0f, 1f)] public float warpChromatic = 0.6f;
        [Tooltip("Camera roll sway (degrees): the 'I feel sick' part, not just a lens effect.")]
        public float warpRoll = 4f;
        [Tooltip("Field of view breathing (degrees).")]
        public float warpFov = 6f;
        public float muffledCutoff = 1200f;

        [Header("80: nearly dark, walking -30 %")]
        public float darkThreshold = 80f;
        [Range(0f, 1f)] public float darkVignette = 0.62f;
        public float darkPostExposure = -2.5f;
        [Range(0f, 1f)] public float darkSpeedMultiplier = 0.7f;

        [Header("100: collapse")]
        public float collapsePostExposure = -10f;
    }

    /// <summary>Resolved effect values for one moment; applied by SymptomEffects.</summary>
    public struct SymptomState
    {
        public float HumVolume;
        public float Vignette;
        public float LookMultiplier;
        public float LensDistortion;
        public float ChromaticAberration;
        public float LowPassCutoff;
        public float PostExposure;
        public float SpeedMultiplier;
        public float CameraRoll;
        public float FovOffset;

        public const float OpenCutoff = 22000f;

        public static SymptomState Evaluate(SymptomSettings s, float level, bool collapsed, float time)
        {
            var hum = Weight(s, level, s.humThreshold);
            var narrow = Weight(s, level, s.narrowThreshold);
            var warp = Weight(s, level, s.warpThreshold);
            var dark = Weight(s, level, s.darkThreshold);

            var state = new SymptomState
            {
                HumVolume = s.humMaxVolume * hum * Mathf.Lerp(0.5f, 1f, level / 100f),
                Vignette = Mathf.Lerp(Mathf.Lerp(s.baseVignette, s.narrowVignette, narrow), s.darkVignette, dark),
                LookMultiplier = Mathf.Lerp(1f, s.narrowLookMultiplier, narrow),
                // Two incommensurate sines: a sway that never settles into a rhythm.
                LensDistortion = s.warpDistortion * warp * Wave(time * s.warpSpeed),
                CameraRoll = s.warpRoll * warp * Wave(time * s.warpSpeed * 1.3f + 1f),
                FovOffset = s.warpFov * warp * Wave(time * s.warpSpeed * 0.8f + 2f),
                ChromaticAberration = s.warpChromatic * warp,
                LowPassCutoff = Mathf.Lerp(OpenCutoff, s.muffledCutoff, warp),
                PostExposure = s.darkPostExposure * dark,
                SpeedMultiplier = Mathf.Lerp(1f, s.darkSpeedMultiplier, dark),
            };

            if (collapsed)
            {
                state.PostExposure = s.collapsePostExposure;
                state.SpeedMultiplier = 0f;
                state.LookMultiplier = 0f;
            }
            return state;
        }

        private static float Wave(float t) => (Mathf.Sin(t) + 0.5f * Mathf.Sin(t * 2.37f)) / 1.5f;

        /// <summary>0 below (threshold − ramp), 1 at the threshold and above, smooth between.</summary>
        public static float Weight(SymptomSettings s, float level, float threshold) =>
            Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(threshold - s.ramp, threshold, level));
    }
}
