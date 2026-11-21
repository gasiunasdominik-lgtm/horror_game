using TMPro;
using UnityEngine;
using Vchod.Core;

namespace Vchod.UI
{
    /// <summary>Speech at the bottom of the screen. The newest line replaces the old one.</summary>
    public sealed class SubtitleView : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float fade = 0.3f;

        private float _until;
        private float _shownFor;

        private void OnEnable() => EventBus.Subscribe<SubtitleRequested>(OnLine);
        private void OnDisable() => EventBus.Unsubscribe<SubtitleRequested>(OnLine);

        private void OnLine(SubtitleRequested e)
        {
            text.text = e.Text;
            _shownFor = SubtitleTiming.Duration(e.Text, e.Duration);
            _until = Time.time + _shownFor;
        }

        private void Update()
        {
            var left = _until - Time.time;
            var alpha = left <= 0f ? 0f : Mathf.Clamp01(Mathf.Min(left, _shownFor - left) / fade);
            text.alpha = alpha;
            text.enabled = alpha > 0f;
        }
    }
}
