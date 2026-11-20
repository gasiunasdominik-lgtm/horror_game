using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Vchod.Core;
using Vchod.World;

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

        private int _zoneCursor;
        private readonly List<int> _zones = new();

        /// <summary>Cycles stairwell → window → CO source → player door, facing the feature.</summary>
        private void TeleportToNextZone()
        {
            var ctx = DebugContext.Instance;
            var house = ctx.House;
            if (house == null || house.Plan == null || ctx.Player == null)
            {
                Report("Teleport: no house in scene");
                return;
            }

            var plan = house.Plan;
            _zones.Clear();
            _zones.Add(LayoutPlan.StartIndex);
            AddZone(plan.FindFeature(CellFeature.Window));
            AddZone(plan.CoSourceIndex);
            AddZone(plan.FindFeature(CellFeature.PlayerDoor));

            _zoneCursor = (_zoneCursor + 1) % _zones.Count;
            var index = _zones[_zoneCursor];
            var cell = plan.Cells[index];
            var yaw = index == LayoutPlan.StartIndex ? Direction.North.Yaw() : cell.FeatureSide.Yaw();
            ctx.Player.Teleport(house.GetCellCenter(index), yaw);
            Report($"Teleport: {(cell.IsCoSource ? "CO source" : cell.Feature.ToString())} {cell.Pos}");
        }

        private void AddZone(int index)
        {
            if (index >= 0 && !_zones.Contains(index)) _zones.Add(index);
        }

        private void ForceReshuffle()
        {
            var house = DebugContext.Instance.House;
            if (house == null)
            {
                Report("Reshuffle: no house in scene");
                return;
            }
            house.Reshuffle();
            Report($"Reshuffle #{house.ReshuffleIndex} hash {house.LayoutHash:X8}");
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
