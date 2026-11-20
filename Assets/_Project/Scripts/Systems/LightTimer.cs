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
            if (_logic.Tick(Time.deltaTime)) EventBus.Publish(new LightTurnedOff());
        }

        public void Press()
        {
            _logic.Duration = config.duration;
            _logic.Press();
            EventBus.Publish(new LightTurnedOn { Duration = config.duration });
        }

        public void ForceOff()
        {
            if (_logic.ForceOff()) EventBus.Publish(new LightTurnedOff());
        }
    }
}
