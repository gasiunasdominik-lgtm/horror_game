using TMPro;
using UnityEngine;
using Vchod.Player;

namespace Vchod.UI
{
    /// <summary>Shows the crosshair and the action of whatever the player is looking at.</summary>
    public sealed class InteractionPromptView : MonoBehaviour
    {
        [SerializeField] private PlayerInteractor interactor;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private CanvasGroup crosshair;
        [SerializeField, Range(0f, 1f)] private float idleCrosshairAlpha = 0.35f;

        private string _shown;

        private void Update()
        {
            var prompt = interactor != null && interactor.Current != null ? interactor.Current.Prompt : null;
            if (ReferenceEquals(prompt, _shown)) return;

            _shown = prompt;
            promptText.text = prompt ?? string.Empty;
            promptText.enabled = prompt != null;
            if (crosshair != null) crosshair.alpha = prompt != null ? 1f : idleCrosshairAlpha;
        }
    }
}
