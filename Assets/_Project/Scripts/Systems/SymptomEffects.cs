using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Vchod.Player;

namespace Vchod.Systems
{
    /// <summary>
    /// Shows the CO level on screen, in sound and in movement. Uses its own high-priority
    /// Volume built at runtime, so the baseline look profile asset is never modified.
    /// </summary>
    public sealed class SymptomEffects : MonoBehaviour
    {
        private const int VolumePriority = 10;

        [SerializeField] private CoSystem co;
        [SerializeField] private PlayerController player;
        [SerializeField] private AudioLowPassFilter lowPass;
        [SerializeField] private AudioSource hum;

        private VolumeProfile _profile;
        private Vignette _vignette;
        private LensDistortion _distortion;
        private ChromaticAberration _chromatic;
        private ColorAdjustments _color;

        public SymptomState Current { get; private set; }

        private void Awake()
        {
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            _vignette = Override<Vignette>();
            _vignette.intensity.overrideState = true;
            _distortion = Override<LensDistortion>();
            _distortion.intensity.overrideState = true;
            _chromatic = Override<ChromaticAberration>();
            _chromatic.intensity.overrideState = true;
            _color = Override<ColorAdjustments>();
            _color.postExposure.overrideState = true;

            var go = new GameObject("SymptomVolume");
            go.transform.SetParent(transform, false);
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = VolumePriority;
            volume.sharedProfile = _profile;

            hum.clip = Audio.ProceduralAudio.CreateTinnitus();
            hum.loop = true;
            hum.volume = 0f;
            hum.Play();
        }

        private T Override<T>() where T : VolumeComponent
        {
            var c = _profile.Add<T>();
            c.active = true;
            return c;
        }

        private void OnDestroy()
        {
            if (_profile != null) Destroy(_profile);
        }

        private void Update()
        {
            var model = co.Model;
            var state = SymptomState.Evaluate(co.Config.symptoms, model.Level, model.IsCollapsed, Time.time);
            Current = state;

            _vignette.intensity.value = state.Vignette;
            _distortion.intensity.value = state.LensDistortion;
            _chromatic.intensity.value = state.ChromaticAberration;
            _color.postExposure.value = state.PostExposure;

            player.SpeedMultiplier = state.SpeedMultiplier;
            player.LookMultiplier = state.LookMultiplier;

            hum.volume = state.HumVolume;
            if (lowPass != null) lowPass.cutoffFrequency = state.LowPassCutoff;
        }
    }
}
