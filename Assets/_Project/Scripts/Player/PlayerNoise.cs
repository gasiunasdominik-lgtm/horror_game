using UnityEngine;
using Vchod.Core;

namespace Vchod.Player
{
    /// <summary>Footsteps as noise events: walking is quiet, sprinting carries across the floor.</summary>
    public sealed class PlayerNoise : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;
        [Tooltip("2D source for the player's own footsteps.")]
        [SerializeField] private AudioSource steps;
        [SerializeField] private float walkVolume = 0.35f;
        [SerializeField] private float runVolume = 0.7f;

        private AudioClip[] _walk;
        private AudioClip[] _run;
        private int _stepIndex;

        private void Awake()
        {
            _walk = new AudioClip[3];
            _run = new AudioClip[3];
            for (var i = 0; i < 3; i++)
            {
                _walk[i] = Audio.ProceduralAudio.CreateShoeStep(i, running: false);
                _run[i] = Audio.ProceduralAudio.CreateShoeStep(i, running: true);
            }
        }

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
            var running = controller.IsSprinting;
            if (steps != null)
            {
                _stepIndex = (_stepIndex + 1) % _walk.Length;
                steps.pitch = 0.94f + Random.value * 0.12f;
                steps.PlayOneShot(running ? _run[_stepIndex] : _walk[_stepIndex], running ? runVolume : walkVolume);
            }
            EventBus.Publish(new NoiseMade
            {
                Position = pos,
                Radius = controller.IsSprinting ? config.sprintNoiseRadius : config.walkNoiseRadius,
            });
        }
    }
}
