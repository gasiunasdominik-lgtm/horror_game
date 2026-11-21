using System.Collections;
using UnityEngine;
using Vchod.Core;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// Neighbour 1, the lady with the cake (GDD). Knock, she opens a crack and invites you in.
    /// At the CO source she is drowsy and complains of a headache: going in is ending 1.
    /// Elsewhere cold air blows from her flat and she lets you through her fire door, one floor up.
    /// The difference is fair: detector, symptoms, her words, the draft.
    /// </summary>
    public sealed class Neighbour : MonoBehaviour, IInteractable
    {
        private enum Phase { Closed, Opening, Open, Gone }

        [SerializeField] private Transform leaf;
        [SerializeField] private Light spill;
        [SerializeField] private AudioSource knock;
        [SerializeField] private AudioSource draft;
        [SerializeField] private float openAngle = 30f;
        [SerializeField] private float answerDelay = 1.6f;
        [SerializeField] private float knockNoiseRadius = 6f;
        [SerializeField] private float closeDistance = 5f;
        [SerializeField] private float lineGap = 0.4f;

        private static readonly string[] TrapLines =
        {
            "Á, to jsi ty. Pojď dál, upekla jsem dort.",
            "Jsem dneska nějaká ospalá... a hlava mě třeští.",
            "Pojď si ke mně na chvíli sednout. Odpočineš si.",
        };
        private static readonly string[] HelpLines =
        {
            "Kdo je? ... Jé, ty jsi od vedle. Celý dům je dneska vzhůru nohama.",
            "Mám otevřené okno, nějak se tu nedalo dýchat. Dort se mi zase spálil.",
            "Mám klíč od požárních dveří, vedou o patro výš. Pojď, projdi u mě.",
        };
        private const string TrapAgain = "Pojď dál, dort ti vystydne.";
        private const string HelpAgain = "Tak pojď, ať tu nestojíš na chodbě.";
        private const string NobodyHome = "Nikdo neotvírá.";

        private ModuleLayoutBuilder _house;
        private PlayerController _player;
        private bool _trap;
        private bool _metBefore;
        private Phase _phase;
        private float _angle;

        public bool IsTrap => _trap;

        public string Prompt => _phase switch
        {
            Phase.Closed => "E  Zaklepat",
            Phase.Open => "E  Vejít",
            Phase.Gone => "E  Zaklepat",
            _ => null,
        };

        public void Setup(ModuleLayoutBuilder house, PlayerController player, bool trap, bool alreadyHelped)
        {
            _house = house;
            _player = player;
            _trap = trap;
            _phase = alreadyHelped ? Phase.Gone : Phase.Closed;
            if (knock != null) knock.clip = Audio.ProceduralAudio.CreateKnock();
            if (draft != null && !trap) draft.clip = Audio.ProceduralAudio.CreateWind();
        }

        public void Interact(GameObject interactor)
        {
            switch (_phase)
            {
                case Phase.Closed:
                case Phase.Gone:
                    Knock();
                    break;
                case Phase.Open:
                    Enter();
                    break;
            }
        }

        private void Knock()
        {
            if (knock != null) knock.Play();
            EventBus.Publish(new NoiseMade { Position = transform.position, Radius = knockNoiseRadius });
            if (_phase == Phase.Gone)
            {
                StartCoroutine(Say(new[] { NobodyHome }, answerDelay + 0.6f));
                return;
            }
            _phase = Phase.Opening;
            StartCoroutine(Answer());
        }

        private IEnumerator Answer()
        {
            yield return new WaitForSeconds(answerDelay);
            _phase = Phase.Open;
            if (spill != null) spill.enabled = true;
            if (draft != null && draft.clip != null) draft.Play();
            var lines = _metBefore ? new[] { _trap ? TrapAgain : HelpAgain } : _trap ? TrapLines : HelpLines;
            _metBefore = true;
            yield return Say(lines, 0f);
        }

        private IEnumerator Say(string[] lines, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);
            foreach (var line in lines)
            {
                var time = SubtitleTiming.ReadTime(line);
                EventBus.Publish(new SubtitleRequested { Text = line, Duration = time });
                // Muffled voice through the door crack: you hear her talk, the words are the subtitle.
                if (knock != null) knock.PlayOneShot(Audio.ProceduralAudio.CreateMumble(line.GetHashCode(), time * 0.85f, _trap));
                yield return new WaitForSeconds(time + lineGap);
                if (_phase == Phase.Closed) yield break; // walked away
            }
        }

        private void Enter()
        {
            Close(Phase.Gone);
            if (_trap) EventBus.Publish(new EnteredFlat { Home = false });
            else _house.TakeShortcut();
        }

        private void Close(Phase next)
        {
            StopAllCoroutines();
            _phase = next;
            if (spill != null) spill.enabled = false;
            if (draft != null) draft.Stop();
        }

        private void Update()
        {
            if (_phase == Phase.Open && _player != null &&
                (_player.transform.position - transform.position).sqrMagnitude > closeDistance * closeDistance)
                Close(Phase.Closed);

            var target = _phase == Phase.Open ? openAngle : 0f;
            _angle = Mathf.MoveTowards(_angle, target, 60f * Time.deltaTime);
            if (leaf != null) leaf.localRotation = Quaternion.Euler(0f, -_angle, 0f);
        }
    }
}
