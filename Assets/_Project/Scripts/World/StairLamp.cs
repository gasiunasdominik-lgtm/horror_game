using UnityEngine;
using Vchod.Core;

namespace Vchod.World
{
    /// <summary>A ceiling lamp on the staircase circuit. Swaps shared materials (no per-instance copies).</summary>
    public sealed class StairLamp : MonoBehaviour
    {
        [SerializeField] private Light lampLight;
        [SerializeField] private Renderer bulb;
        [SerializeField] private Material onMaterial;
        [SerializeField] private Material offMaterial;
        [Tooltip("Quiet mains hum of the tube while it is on.")]
        [SerializeField] private AudioSource hum;

        private static AudioClip _humClip;
        private bool _on;
        private float _onSince;
        private bool _lit;
        private int _seed;

        private void Awake()
        {
            var p = transform.position;
            _seed = Mathf.RoundToInt(p.x * 73f + p.y * 31f + p.z * 151f);
            if (hum != null)
            {
                if (_humClip == null) _humClip = Audio.ProceduralAudio.CreateTubeHum();
                hum.clip = _humClip;
                hum.loop = true;
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<LightTurnedOn>(OnLightOn);
            EventBus.Subscribe<LightTurnedOff>(OnLightOff);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<LightTurnedOn>(OnLightOn);
            EventBus.Unsubscribe<LightTurnedOff>(OnLightOff);
        }

        private void OnLightOn(LightTurnedOn e) => SetOn(true);
        private void OnLightOff(LightTurnedOff e) => SetOn(false);

        public void SetOn(bool on)
        {
            _on = on;
            _onSince = Time.time;
            Apply(on && TubeFlicker.IsLit(0f, _seed));
            if (hum == null) return;
            if (on && !hum.isPlaying) hum.Play();
            if (!on) hum.Stop();
        }

        private void Update()
        {
            if (!_on) return;
            var lit = TubeFlicker.IsLit(Time.time - _onSince, _seed);
            if (lit != _lit) Apply(lit);
        }

        private void Apply(bool lit)
        {
            _lit = lit;
            lampLight.enabled = lit;
            bulb.sharedMaterial = lit ? onMaterial : offMaterial;
        }
    }
}
