using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Vchod.Core;
using Vchod.Player;
using Vchod.Systems;

namespace Vchod.UI
{
    /// <summary>
    /// Reading a story piece: the game pauses, the text is shown on a dim panel, any of
    /// E / Enter / Esc / gamepad South closes it. Reading is never a race against the firefighter.
    /// </summary>
    public sealed class StoryView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text counter;
        [SerializeField] private PlayerController player;
        [SerializeField] private float closeGuard = 0.4f;

        private float _openedAt = -1f;
        private float _timeScale = 1f;

        public bool IsOpen => _openedAt >= 0f;

        private void Awake() => Hide();
        private void OnEnable() => EventBus.Subscribe<StoryPieceFound>(OnFound);
        private void OnDisable() => EventBus.Unsubscribe<StoryPieceFound>(OnFound);

        private void OnFound(StoryPieceFound e)
        {
            var entry = StoryCatalog.ById(e.Id);
            if (entry == null) return;
            title.text = entry.Title;
            body.text = entry.Text;
            counter.text = $"Kousky příběhu {StoryProgress.Collected.Count}/{StoryCatalog.All.Count}{(e.New ? "   · nový" : "")}";
            group.alpha = 1f;
            _openedAt = Time.unscaledTime;
            _timeScale = Time.timeScale;
            Time.timeScale = 0f;
            if (player != null) player.SetInputLock(this, true);
        }

        private void Update()
        {
            if (!IsOpen || Time.unscaledTime - _openedAt < closeGuard) return;
            var k = Keyboard.current;
            var g = Gamepad.current;
            if ((k != null && (k.eKey.wasPressedThisFrame || k.enterKey.wasPressedThisFrame || k.escapeKey.wasPressedThisFrame)) ||
                (g != null && (g.buttonSouth.wasPressedThisFrame || g.buttonEast.wasPressedThisFrame)))
                Close();
        }

        public void Close()
        {
            if (!IsOpen) return;
            Time.timeScale = _timeScale <= 0f ? 1f : _timeScale;
            if (player != null) player.SetInputLock(this, false);
            Hide();
        }

        private void Hide()
        {
            _openedAt = -1f;
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }
}
