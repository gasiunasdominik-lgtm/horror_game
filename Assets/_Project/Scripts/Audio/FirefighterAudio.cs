using UnityEngine;
using UnityEngine.AI;
using Vchod.Core;
using Vchod.Enemy;

namespace Vchod.Audio
{
    /// <summary>
    /// What the player hears of the firefighter (GDD: heavy mask breathing, gear, torch):
    /// all 3D, so his position is readable without seeing him. Faster and louder in a chase.
    /// </summary>
    public sealed class FirefighterAudio : MonoBehaviour
    {
        private const int StepVariants = 3;

        [SerializeField] private FirefighterController firefighter;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private AudioSource breath;
        [SerializeField] private AudioSource steps;
        [SerializeField] private AudioSource voice;
        [SerializeField] private float stepLength = 0.8f;
        [SerializeField] private float calmBreathPitch = 0.95f;
        [SerializeField] private float chaseBreathPitch = 1.35f;

        private AudioClip[] _steps;
        private AudioClip _call;
        private float _travelled;
        private Vector3 _last;
        private int _stepIndex;

        private void Awake()
        {
            breath.clip = ProceduralAudio.CreateMaskBreath();
            breath.loop = true;
            breath.Play();
            _steps = new AudioClip[StepVariants];
            for (var i = 0; i < StepVariants; i++) _steps[i] = ProceduralAudio.CreateGearStep(i);
            _call = ProceduralAudio.CreateMaskedCall();
            _last = transform.position;
        }

        private void OnEnable() => EventBus.Subscribe<FirefighterStateChanged>(OnState);
        private void OnDisable() => EventBus.Unsubscribe<FirefighterStateChanged>(OnState);

        private void OnState(FirefighterStateChanged e)
        {
            if (e.State == FirefighterState.Call) voice.PlayOneShot(_call);
        }

        private void Update()
        {
            var chasing = firefighter.State == FirefighterState.Chase;
            breath.pitch = Mathf.MoveTowards(breath.pitch, chasing ? chaseBreathPitch : calmBreathPitch, Time.deltaTime * 0.5f);

            var pos = transform.position;
            var d = pos - _last;
            d.y = 0f;
            _last = pos;
            if (d.sqrMagnitude > 1f || !agent.enabled) return; // warp, not a step
            _travelled += d.magnitude;
            if (_travelled < stepLength) return;
            _travelled = 0f;
            _stepIndex = (_stepIndex + 1) % _steps.Length;
            steps.PlayOneShot(_steps[_stepIndex], chasing ? 1f : 0.7f);
        }
    }
}
