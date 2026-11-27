using UnityEngine;
using Vchod.Core;

namespace Vchod.Systems
{
    /// <summary>
    /// The building's staircase light. Publishes LightTurnedOn / LightTurnedOff;
    /// lamps and the house reshuffle listen to those events.
    /// </summary>
    public sealed class LightTimer : MonoBehaviour
    {
        [SerializeField] private LightTimerConfig config;

        private LightTimerLogic _logic;

        public bool IsOn => _logic != null && _logic.IsOn;
        public float Remaining => _logic?.Remaining ?? 0f;

        private void Awake() => _logic = new LightTimerLogic(config.duration);

        private void Update()
        {
            if (_logic.Tick(Time.deltaTime))
            {
                PlayRelay();
                EventBus.Publish(new LightTurnedOff());
            }
        }

        private static AudioClip _relay;

        // The timer relay in the stairwell box clacks off: you hear the dark coming.
        private static void PlayRelay()
        {
            var cam = Camera.main;
            if (cam == null) return;
            if (_relay == null) _relay = Audio.ProceduralAudio.CreateSwitchClick(press: false);
            AudioSource.PlayClipAtPoint(_relay, cam.transform.position + Vector3.up * 0.8f, 0.35f);
        }

        public void Press(Vector3 position)
        {
            _logic.Duration = config.duration;
            _logic.Press();
            EventBus.Publish(new LightTurnedOn { Duration = config.duration, Position = position });
        }

        public void ForceOff()
        {
            if (_logic.ForceOff()) EventBus.Publish(new LightTurnedOff());
        }
    }
}
