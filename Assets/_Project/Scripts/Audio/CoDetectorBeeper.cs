using UnityEngine;
using Vchod.Core;
using Vchod.Systems;
using Vchod.World;

namespace Vchod.Audio
{
    /// <summary>
    /// The neighbour's CO alarm, heard through the wall. A 3D source at the leak gives
    /// direction and loudness; the interval shortens as the player gets closer (fair cue).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class CoDetectorBeeper : MonoBehaviour
    {
        [SerializeField] private CoSystem co;
        [SerializeField] private ModuleLayoutBuilder house;

        private AudioSource _source;
        private DetectorSettings _settings;
        private float _nextBeep;

        private void Awake()
        {
            _settings = co.Config.detector;
            _source = GetComponent<AudioSource>();
            _source.clip = ProceduralAudio.CreateBeep(_settings.frequency, _settings.beepLength);
            _source.playOnAwake = false;
            _source.spatialBlend = 1f;
            _source.rolloffMode = AudioRolloffMode.Linear;
            _source.minDistance = 1.5f;
            _source.maxDistance = _settings.range + 6f;
            _source.volume = _settings.volume;
        }

        private void OnEnable() => EventBus.Subscribe<LayoutRebuilt>(OnLayoutRebuilt);
        private void OnDisable() => EventBus.Unsubscribe<LayoutRebuilt>(OnLayoutRebuilt);

        private void OnLayoutRebuilt(LayoutRebuilt e) => transform.position = house.CoSourcePosition;

        public float CurrentInterval => _settings.IntervalFor(co.DistanceToSource);

        private void Update()
        {
            if (house.Plan == null || Time.time < _nextBeep) return;
            _source.Play();
            _nextBeep = Time.time + CurrentInterval;
        }
    }
}
