using System.Collections;
using UnityEngine;
using Vchod.Core;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// Neighbour 3 (GDD): an argument behind a closed door. Nobody opens; on the second knock the
    /// player notices it repeats word for word: a TV left on. Its noise covers your steps nearby,
    /// so it is also a place to slip past the firefighter.
    /// </summary>
    [RequireComponent(typeof(NoiseMask))]
    public sealed class ArguingFlat : MonoBehaviour, IInteractable
    {
        [SerializeField] private AudioSource voices;
        [SerializeField] private AudioSource knock;
        [SerializeField] private float knockNoiseRadius = 6f;

        private static readonly string[] Lines =
        {
            "Nikdo neotvírá. Hádají se dál, jako by tě neslyšeli.",
            "Počkej... tuhle větu už říkala. Slovo od slova stejně.",
            "Pořád dokola. Znělo to jako televize puštěná moc nahlas.",
        };

        private int _knocks;
        private bool _busy;

        public string Prompt => _busy ? null : "E  Zaklepat";

        private void Start()
        {
            if (voices == null) return;
            voices.clip = Audio.ProceduralAudio.CreateArgument();
            voices.time = Random.Range(0f, voices.clip.length * 0.9f);
            voices.Play();
            if (knock != null) knock.clip = Audio.ProceduralAudio.CreateKnock();
        }

        public void Interact(GameObject interactor)
        {
            if (_busy) return;
            if (knock != null) knock.Play();
            EventBus.Publish(new NoiseMade { Position = transform.position, Radius = knockNoiseRadius });
            StartCoroutine(Answer(Lines[Mathf.Min(_knocks, Lines.Length - 1)]));
            _knocks++;
        }

        private IEnumerator Answer(string line)
        {
            _busy = true;
            yield return new WaitForSeconds(1.8f);
            EventBus.Publish(new SubtitleRequested { Text = line, Duration = SubtitleTiming.ReadTime(line) });
            _busy = false;
        }
    }
}
