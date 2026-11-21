using UnityEngine;
using Vchod.Player;

namespace Vchod.World
{
    /// <summary>
    /// The lift. The firefighters switched it off (GDD canon), so the call button only blinks.
    /// One more small true detail the player can understand later.
    /// </summary>
    public sealed class ElevatorDoor : MonoBehaviour, IInteractable
    {
        [SerializeField] private Renderer callLight;
        [SerializeField] private Material lightOff;
        [SerializeField] private Material lightOn;
        [SerializeField] private float blinkTime = 1.2f;
        [SerializeField] private string callPrompt = "E  Přivolat výtah";
        [SerializeField] private string deadPrompt = "Výtah nejede";

        private bool _tried;
        private float _blinkUntil = -1f;

        public string Prompt => _tried ? deadPrompt : callPrompt;

        public void Interact(GameObject interactor)
        {
            _tried = true;
            _blinkUntil = Time.time + blinkTime;
        }

        private void Update()
        {
            if (callLight == null) return;
            var on = Time.time < _blinkUntil && Mathf.Repeat(Time.time * 6f, 1f) < 0.5f;
            var mat = on ? lightOn : lightOff;
            if (callLight.sharedMaterial != mat) callLight.sharedMaterial = mat;
        }
    }
}
