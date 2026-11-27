using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vchod.Core;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// Neighbour 2 (GDD): the man with the radio. Gruff but on your side: he heard the "man in the
    /// mask" (and says where), tells you an open window helps, and his radio news drops a
    /// half-heard hint of the truth. Always honest: pillar 1 (trust the body), pillar 2 (fear → understanding).
    /// </summary>
    public sealed class RadioNeighbour : MonoBehaviour, IInteractable
    {
        private enum Phase { Closed, Opening, Open }

        [SerializeField] private Transform leaf;
        [SerializeField] private Light spill;
        [SerializeField] private AudioSource knock;
        [SerializeField] private AudioSource radio;
        [SerializeField] private float openAngle = 25f;
        [SerializeField] private float answerDelay = 2.2f;
        [SerializeField] private float knockNoiseRadius = 6f;
        [SerializeField] private float closeDistance = 5f;
        [SerializeField] private float lineGap = 0.4f;

        private const string Greeting = "Co je? ... Jo, ty. Taky nemůžeš spát?";
        private const string Window = "Jestli se ti točí hlava, otevři okno. Mně to vždycky pomůže.";
        private const string Radio = "[rádio, šum] ...hasiči zasahují v ... domě na ... evakuace ... nevstupujte do ...";
        private const string Again = "Už jsem ti to říkal. Okno. A dej si bacha na toho v masce.";

        private PlayerController _player;
        private Phase _phase;
        private float _angle;
        private bool _met;

        public string Prompt => _phase == Phase.Closed ? "E  Zaklepat" : null;

        public void Setup(PlayerController player) => _player = player;

        private void Start()
        {
            if (knock != null) knock.clip = Audio.ProceduralAudio.CreateKnock();
            if (radio == null) return;
            radio.clip = Audio.ProceduralAudio.CreateRadio();
            radio.volume = 0.18f; // muffled behind the closed door
            radio.Play();
        }

        public void Interact(GameObject interactor)
        {
            if (_phase != Phase.Closed) return;
            if (knock != null) knock.Play();
            EventBus.Publish(new NoiseMade { Position = transform.position, Radius = knockNoiseRadius });
            _phase = Phase.Opening;
            StartCoroutine(Answer());
        }

        /// <summary>What he heard of the firefighter, by floor. Pure, for tests.</summary>
        public static string MaskLine(int firefighterFloor, int myFloor)
        {
            if (firefighterFloor < 0) return "Venku houkaly sirény. Pak nic. Divná noc.";
            if (firefighterFloor > myFloor) return "Nahoře někdo chodí. V takový masce, funí jak lokomotiva.";
            if (firefighterFloor < myFloor) return "Dole někdo chodí. V masce. Funí jak lokomotiva.";
            return "Před chvílí tu někdo šel. V masce. Klepal na dveře. Neotvíral jsem.";
        }

        private IEnumerator Answer()
        {
            yield return new WaitForSeconds(answerDelay);
            _phase = Phase.Open;
            if (spill != null) spill.enabled = true;
            if (radio != null) radio.volume = 0.4f;
            var lines = new List<string>();
            if (_met) lines.Add(Again);
            else
            {
                lines.Add(Greeting);
                lines.Add(MaskLine(HouseState.FirefighterFloor, FloorOfHouse()));
                lines.Add(Window);
                lines.Add(Radio);
            }
            _met = true;
            foreach (var line in lines)
            {
                var time = SubtitleTiming.ReadTime(line);
                EventBus.Publish(new SubtitleRequested { Text = line, Duration = time });
                if (knock != null && !line.StartsWith("[")) knock.PlayOneShot(Audio.ProceduralAudio.CreateMumble(line.GetHashCode(), time * 0.8f, false), 0.8f);
                yield return new WaitForSeconds(time + lineGap);
                if (_phase != Phase.Open) yield break;
            }
        }

        private int FloorOfHouse()
        {
            var house = GetComponentInParent<ModuleLayoutBuilder>();
            return house != null ? house.Floor : 0;
        }

        private void Close()
        {
            StopAllCoroutines();
            _phase = Phase.Closed;
            if (spill != null) spill.enabled = false;
            if (radio != null) radio.volume = 0.18f;
        }

        private void Update()
        {
            if (_phase == Phase.Open && _player != null &&
                (_player.transform.position - transform.position).sqrMagnitude > closeDistance * closeDistance)
                Close();
            var target = _phase == Phase.Open ? openAngle : 0f;
            _angle = Mathf.MoveTowards(_angle, target, 60f * Time.deltaTime);
            if (leaf != null) leaf.localRotation = Quaternion.Euler(0f, -_angle, 0f);
        }
    }
}
