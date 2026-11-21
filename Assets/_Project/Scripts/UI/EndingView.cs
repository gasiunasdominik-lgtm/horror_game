using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Vchod.Core;
using Vchod.Systems;

namespace Vchod.UI
{
    /// <summary>
    /// Fades to black, shows the ending, then offers a new round (new house) or the same seed.
    /// </summary>
    public sealed class EndingView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text hint;
        [SerializeField] private float fadeTime = 2.5f;
        [SerializeField] private float hintDelay = 4f;

        private float _shownAt = -1f;

        private void Awake()
        {
            group.alpha = 0f;
            hint.enabled = false;
            AudioListener.pause = false;
            AudioListener.volume = 1f;
        }

        private void OnEnable() => EventBus.Subscribe<RoundEnded>(OnEnded);
        private void OnDisable() => EventBus.Unsubscribe<RoundEnded>(OnEnded);

        private void OnEnded(RoundEnded e)
        {
            title.text = EndingTexts.Title(e.Ending);
            body.text = EndingTexts.Body(e.Ending, e.Cause);
            hint.enabled = false;
            _shownAt = Time.unscaledTime;
        }

        private void Update()
        {
            if (_shownAt < 0f) return;
            var t = Time.unscaledTime - _shownAt;
            group.alpha = Mathf.Clamp01(t / fadeTime);
            AudioListener.volume = 1f - group.alpha;
            if (t < hintDelay) return;
            hint.enabled = true;

            var k = Keyboard.current;
            var g = Gamepad.current;
            if ((k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame)) ||
                (g != null && g.buttonSouth.wasPressedThisFrame))
                GameBootstrap.RestartRound(SeedService.NewRandomSeed());
            else if ((k != null && k.rKey.wasPressedThisFrame) || (g != null && g.buttonWest.wasPressedThisFrame))
                GameBootstrap.RestartRound();
        }
    }
}
