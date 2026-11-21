using UnityEngine;
using Vchod.Core;

namespace Vchod.Player
{
    /// <summary>Footsteps as noise events: walking is quiet, sprinting carries across the floor.</summary>
    public sealed class PlayerNoise : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;

        private Vector3 _last;
        private float _travelled;

        private void OnEnable() => _last = transform.position;

        private void Update()
        {
            var pos = transform.position;
            var delta = pos - _last;
            delta.y = 0f;
            _last = pos;
            // Teleports (reshuffle, debug) are not steps.
            if (delta.sqrMagnitude > 1f) return;

            _travelled += delta.magnitude;
            var config = controller.Config;
            if (_travelled < config.stepLength) return;
            _travelled = 0f;
            EventBus.Publish(new NoiseMade
            {
                Position = pos,
                Radius = controller.IsSprinting ? config.sprintNoiseRadius : config.walkNoiseRadius,
            });
        }
    }
}
