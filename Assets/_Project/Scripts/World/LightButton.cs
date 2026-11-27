using UnityEngine;
using Vchod.Player;
using Vchod.Systems;

namespace Vchod.World
{
    /// <summary>The glowing staircase light switch. Bound to the LightTimer when the house is built.</summary>
    public sealed class LightButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "E  Rozsvítit";

        [Tooltip("The click carries a little: it is a small noise for the firefighter too.")]
        [SerializeField] private float clickNoiseRadius = 4f;

        private static AudioClip _click;
        private LightTimer _timer;

        public string Prompt => prompt;

        public void Bind(LightTimer timer) => _timer = timer;

        public void Interact(GameObject interactor)
        {
            if (_click == null) _click = Audio.ProceduralAudio.CreateSwitchClick(press: true);
            AudioSource.PlayClipAtPoint(_click, transform.position + Vector3.up * 1.3f, 0.8f);
            Core.EventBus.Publish(new Core.NoiseMade { Position = transform.position, Radius = clickNoiseRadius });
            if (_timer != null) _timer.Press(transform.position);
        }
    }
}
