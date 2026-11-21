using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using Vchod.Core;
using Vchod.Enemy;

namespace Vchod.DebugTools
{
    /// <summary>
    /// Writes one CSV per game session into PlaytestLogs/ so /playtest can reconstruct
    /// what happened (rounds, light, reshuffles, CO stages, endings, quit).
    /// </summary>
    public sealed class PlaytestLogger : MonoBehaviour
    {
        public const string FolderName = "PlaytestLogs";

        private StreamWriter _writer;

        public static PlaytestLogger Instance { get; private set; }
        public string FilePath { get; private set; }

        private void Awake()
        {
            Instance = this;
            try
            {
                var dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", FolderName));
                Directory.CreateDirectory(dir);
                FilePath = Path.Combine(dir, $"session_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.csv");
                _writer = new StreamWriter(FilePath, false) { AutoFlush = true };
                _writer.WriteLine("real_time,round_time,event,seed,detail");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"PlaytestLogger disabled: {e.Message}");
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<RoundStarted>(OnRoundStarted);
            EventBus.Subscribe<LightTurnedOn>(OnLightOn);
            EventBus.Subscribe<LightTurnedOff>(OnLightOff);
            EventBus.Subscribe<LayoutRebuilt>(OnLayoutRebuilt);
            EventBus.Subscribe<CoStageChanged>(OnCoStage);
            EventBus.Subscribe<CoCollapsed>(OnCollapsed);
            EventBus.Subscribe<FirefighterStateChanged>(OnFirefighterState);
            EventBus.Subscribe<PlayerCaught>(OnCaught);
            EventBus.Subscribe<FloorChanged>(OnFloor);
            EventBus.Subscribe<RoundEnded>(OnEnded);
            Application.quitting += OnQuit;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<RoundStarted>(OnRoundStarted);
            EventBus.Unsubscribe<LightTurnedOn>(OnLightOn);
            EventBus.Unsubscribe<LightTurnedOff>(OnLightOff);
            EventBus.Unsubscribe<LayoutRebuilt>(OnLayoutRebuilt);
            EventBus.Unsubscribe<CoStageChanged>(OnCoStage);
            EventBus.Unsubscribe<CoCollapsed>(OnCollapsed);
            EventBus.Unsubscribe<FirefighterStateChanged>(OnFirefighterState);
            EventBus.Unsubscribe<PlayerCaught>(OnCaught);
            EventBus.Unsubscribe<FloorChanged>(OnFloor);
            EventBus.Unsubscribe<RoundEnded>(OnEnded);
            Application.quitting -= OnQuit;
        }

        private void OnDestroy()
        {
            _writer?.Dispose();
            _writer = null;
        }

        private void OnRoundStarted(RoundStarted e) => Log("round_start", $"seed={e.Seed}");
        private void OnLightOn(LightTurnedOn e) => Log("light_on", $"duration={F(e.Duration)};pos={V(e.Position)}");
        private void OnLightOff(LightTurnedOff e) => Log("light_off", string.Empty);
        private void OnLayoutRebuilt(LayoutRebuilt e) => Log("layout", $"index={e.ReshuffleIndex};hash={e.LayoutHash:X8}");
        private void OnCoStage(CoStageChanged e) => Log("co_stage", $"from={e.PreviousStage};to={e.Stage};level={F(e.Level)}");
        private void OnCollapsed(CoCollapsed e) => Log("co_collapse", $"level={F(e.Level)}");
        private void OnFirefighterState(FirefighterStateChanged e) => Log("firefighter", $"from={e.Previous};to={e.State}");
        private void OnCaught(PlayerCaught e) => Log("caught", $"pos={V(e.Position)}");
        private void OnFloor(FloorChanged e) => Log("floor", $"from={e.From};to={e.To};stairs={e.ViaStairs}");
        private void OnEnded(RoundEnded e) => Log("round_end", $"ending={e.Ending};cause={e.Cause}");
        private void OnQuit() => Log("quit", string.Empty);

        private static string V(Vector3 v) => $"{F(v.x)} {F(v.y)} {F(v.z)}";

        private static string F(float v) => v.ToString("0.0", CultureInfo.InvariantCulture);

        /// <summary>Public so cheats and reports leave a trace in the same timeline.</summary>
        public void Log(string evt, string detail)
        {
            if (_writer == null) return;
            var real = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var round = Time.timeSinceLevelLoad.ToString("0.00", CultureInfo.InvariantCulture);
            _writer.WriteLine($"{real},{round},{evt},{SeedService.Seed},{detail.Replace(',', ';')}");
        }
    }
}
