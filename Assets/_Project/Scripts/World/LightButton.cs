using UnityEngine;
using Vchod.Player;
using Vchod.Systems;

namespace Vchod.World
{
    /// <summary>The glowing staircase light switch. Bound to the LightTimer when the house is built.</summary>
    public sealed class LightButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private string prompt = "E  Rozsvítit";

        private LightTimer _timer;

        public string Prompt => prompt;

        public void Bind(LightTimer timer) => _timer = timer;

        public void Interact(GameObject interactor)
        {
            if (_timer != null) _timer.Press();
        }
    }
}
