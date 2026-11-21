using UnityEngine;

namespace Vchod.Player
{
    /// <summary>Finds what the player looks at and uses it on the interact button.</summary>
    public sealed class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private PlayerController controller;

        public IInteractable Current { get; private set; }

        private void Update()
        {
            Current = null;
            if (!controller.InputEnabled) return;

            if (controller.IsHidden)
            {
                // Inside a hiding spot the only action is getting out.
                Current = controller.HiddenIn;
                if (controller.Input.Interact.WasPressedThisFrame()) Current.Interact(gameObject);
                return;
            }

            var config = controller.Config;
            var eye = controller.CameraPivot;
            if (Physics.Raycast(eye.position, eye.forward, out var hit, config.interactDistance,
                    config.interactMask, QueryTriggerInteraction.Ignore))
            {
                Current = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (Current != null && controller.Input.Interact.WasPressedThisFrame())
                Current.Interact(gameObject);
        }
    }
}
