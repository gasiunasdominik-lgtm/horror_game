using UnityEngine;
using UnityEngine.InputSystem;
using Vchod.Core;

namespace Vchod.DebugTools
{
    /// <summary>F2–F10 shortcuts for fast testing (F1 overlay and F8 bug report live in their own components).</summary>
    public sealed class DebugCheats : MonoBehaviour
    {
        private static readonly float[] TimeScales = { 1f, 4f, 0.25f };
        private int _timeScaleIndex;

        private void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            var ctx = DebugContext.Instance;

            if (k.f2Key.wasPressedThisFrame)
            {
                ctx.SetGodMode(!ctx.GodMode);
                Report($"God mode {(ctx.GodMode ? "ON" : "off")}");
            }
            if (k.f3Key.wasPressedThisFrame) TeleportToNextZone();
            if (k.f4Key.wasPressedThisFrame)
            {
                if (k.shiftKey.isPressed) GameBootstrap.RestartRound(SeedService.NewRandomSeed());
                else ForceReshuffle();
            }
            if (k.f5Key.wasPressedThisFrame) Report("Enemy states: not implemented until M1 (T-020)");
            if (k.f6Key.wasPressedThisFrame)
            {
                _timeScaleIndex = (_timeScaleIndex + 1) % TimeScales.Length;
                Time.timeScale = TimeScales[_timeScaleIndex];
                Report($"Time scale {Time.timeScale}");
            }
            if (k.f7Key.wasPressedThisFrame) ToggleFogAndPost();
            if (k.f9Key.wasPressedThisFrame)
            {
                Report($"Restart round, seed {SeedService.Seed}");
                GameBootstrap.RestartRound();
            }
            if (k.f10Key.wasPressedThisFrame) Report("Endings: not implemented until M1 (T-026)");
        }

        private static void Report(string message)
        {
            DebugContext.Instance.Notify(message);
            if (PlaytestLogger.Instance != null) PlaytestLogger.Instance.Log("debug", message);
        }

        private void TeleportToNextZone()
        {
            Report("Teleport: no zones yet");
        }

        private void ForceReshuffle()
        {
            Report("Reshuffle: no house yet");
        }

        private bool _fogAndPostOff;
        private bool _savedFog;

        private void ToggleFogAndPost()
        {
            _fogAndPostOff = !_fogAndPostOff;
            if (_fogAndPostOff) _savedFog = RenderSettings.fog;
            RenderSettings.fog = !_fogAndPostOff && _savedFog;

            var cam = Camera.main;
            if (cam != null)
            {
                var data = cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                if (data != null) data.renderPostProcessing = !_fogAndPostOff;
            }
            Report($"Fog + post-processing {(_fogAndPostOff ? "OFF" : "on")}");
        }
    }
}
