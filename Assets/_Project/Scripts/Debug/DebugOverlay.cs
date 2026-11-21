using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using Vchod.Core;

namespace Vchod.DebugTools
{
    /// <summary>
    /// F1 cycles: hidden → info → info + controls (cursor free, seed entry) → hidden.
    /// IMGUI is allowed here only because this assembly never ships in release builds.
    /// </summary>
    public sealed class DebugOverlay : MonoBehaviour
    {
        private enum Mode { Hidden, Info, Interactive }

        private const float RefreshInterval = 0.25f;
        private const float ToastDuration = 3f;

        private readonly StringBuilder _text = new(512);
        private Mode _mode = Mode.Hidden;
        private float _nextRefresh;
        private string _cached = string.Empty;
        private string _seedInput = string.Empty;
        private string _toast;
        private float _toastUntil;
        private GUIStyle _style;

        private void OnEnable() => DebugContext.Instance.Notified += ShowToast;

        private void OnDisable()
        {
            if (DebugContext.Instance != null) DebugContext.Instance.Notified -= ShowToast;
        }

        private void ShowToast(string message)
        {
            _toast = message;
            _toastUntil = Time.unscaledTime + ToastDuration;
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame) CycleMode();

            if (_mode != Mode.Hidden && Time.unscaledTime >= _nextRefresh)
            {
                _nextRefresh = Time.unscaledTime + RefreshInterval;
                _cached = BuildText();
            }
        }

        private void CycleMode()
        {
            _mode = _mode switch
            {
                Mode.Hidden => Mode.Info,
                Mode.Info => Mode.Interactive,
                _ => Mode.Hidden,
            };
            if (_mode == Mode.Interactive) _seedInput = SeedService.Seed.ToString();
            DebugContext.Instance.SetInputBlocked(this, _mode == Mode.Interactive);
            _nextRefresh = 0f;
        }

        private string BuildText()
        {
            var ctx = DebugContext.Instance;
            var s = ctx.Capture();
            _text.Clear();
            _text.Append("FPS ").Append(ctx.Fps.ToString("0")).Append("  worst ").Append(ctx.WorstFrameMs.ToString("0.0")).Append(" ms\n");
            _text.Append("Seed ").Append(s.seed).Append("  layout ").Append(s.layoutHash).Append(" #").Append(s.reshuffleIndex).Append("  floor ").Append(s.floor).Append('\n');
            _text.Append("CO ").Append(s.coLevel < 0 ? "n/a" : s.coLevel.ToString("0.0")).Append("  stage ").Append(s.coStage < 0 ? "n/a" : s.coStage.ToString()).Append('\n');
            _text.Append("Enemy ").Append(s.enemyState).Append("  light ").Append(s.lightOn ? "ON" : "off").Append('\n');
            _text.Append("Pos ").Append(s.playerPosition.ToString("F1")).Append("  yaw ").Append(s.playerYaw.ToString("0")).Append('\n');
            _text.Append("Round ").Append(s.roundTime.ToString("0.0")).Append(" s  timeScale ").Append(s.timeScale.ToString("0.##"));
            if (s.godMode) _text.Append("  GOD");
            _text.Append('\n');
            _text.Append("F1 overlay  F2 god  F3 teleport (Shift: floor)  F4 reshuffle (Shift: new seed)\n");
            _text.Append("F5 enemy  F6 time  F7 fog/post  F8 bug report  F9 restart  F10 ending preview");
            return _text.ToString();
        }

        private void OnGUI()
        {
            _style ??= new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 14,
                richText = false,
                padding = new RectOffset(8, 8, 6, 6),
            };

            if (_mode != Mode.Hidden)
            {
                var size = _style.CalcSize(new GUIContent(_cached));
                GUI.Box(new Rect(10, 10, size.x, size.y), _cached, _style);

                if (_mode == Mode.Interactive) DrawControls(new Rect(10, 20 + size.y, 360, 60));
            }

            if (_toast != null && Time.unscaledTime < _toastUntil)
            {
                var size = _style.CalcSize(new GUIContent(_toast));
                GUI.Box(new Rect((Screen.width - size.x) * 0.5f, Screen.height - size.y - 40, size.x, size.y), _toast, _style);
            }
        }

        private void DrawControls(Rect area)
        {
            GUILayout.BeginArea(area, GUI.skin.box);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Seed", GUILayout.Width(40));
            _seedInput = GUILayout.TextField(_seedInput, 9, GUILayout.Width(100));
            if (GUILayout.Button("Restart with seed") && int.TryParse(_seedInput, out var seed))
            {
                DebugContext.Instance.Notify($"Restart with seed {seed}");
                GameBootstrap.RestartRound(seed);
            }
            if (GUILayout.Button("Random"))
                _seedInput = SeedService.NewRandomSeed().ToString();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
