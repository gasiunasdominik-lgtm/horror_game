using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Vchod.Core;
using Vchod.World;

namespace Vchod.DebugTools
{
    /// <summary>
    /// Benchmark run for milestone measurements: <c>-perf N</c> plays N seconds with the
    /// light on, visiting every cell and turning around, uncapped frame rate, then writes
    /// PlaytestLogs/perf_*.txt and quits. Same seed (-seed) = same route.
    /// </summary>
    public sealed class PerfProbe : MonoBehaviour
    {
        public const string Argument = "-perf";
        private const float Warmup = 3f;
        private const float SecondsPerCell = 2.5f;
        private const float TurnSpeed = 144f;

        public static bool TryGetDuration(out float seconds)
        {
            seconds = 0f;
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
                if (string.Equals(args[i], Argument, StringComparison.OrdinalIgnoreCase))
                    return float.TryParse(args[i + 1], NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
            return false;
        }

        public IEnumerator Run(float duration)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
            yield return new WaitForSeconds(Warmup);

            var ctx = DebugContext.Instance;
            ctx.SetGodMode(true);
            var frames = new List<float>(20000);
            var t = 0f;
            var nextCell = 0f;
            var cell = 0;

            while (t < duration)
            {
                var house = ctx.House;
                if (ctx.LightTimer != null && !ctx.LightTimer.IsOn) ctx.LightTimer.Press(ctx.Player.transform.position);
                if (house != null && house.Plan != null && t >= nextCell)
                {
                    cell = (cell + 1) % house.Plan.Count;
                    ctx.Player.Teleport(house.GetCellCenter(cell), ctx.Player.transform.eulerAngles.y);
                    nextCell = t + SecondsPerCell;
                }
                ctx.Player.transform.Rotate(0f, TurnSpeed * Time.unscaledDeltaTime, 0f);

                yield return null;
                frames.Add(Time.unscaledDeltaTime * 1000f);
                t += Time.unscaledDeltaTime;
            }

            Write(frames, duration);
            Application.Quit();
        }

        private static void Write(List<float> frames, float duration)
        {
            frames.Sort();
            var sum = 0f;
            foreach (var f in frames) sum += f;
            var avgMs = sum / frames.Count;
            var p99 = frames[(int)(frames.Count * 0.99f)];
            var worst = frames[frames.Count - 1];
            var low1 = 0f;
            var lowCount = Math.Max(1, frames.Count / 100);
            for (var i = frames.Count - lowCount; i < frames.Count; i++) low1 += frames[i];
            low1 /= lowCount;

            var house = DebugContext.Instance.House;
            var c = CultureInfo.InvariantCulture;
            var lines = new[]
            {
                $"date={DateTime.Now:yyyy-MM-dd HH:mm}",
                $"git={BuildInfo.GitHash}",
                $"device={SystemInfo.deviceModel}",
                $"gpu={SystemInfo.graphicsDeviceName}",
                $"cpu={SystemInfo.processorType}",
                $"resolution={Screen.width}x{Screen.height} fullscreen={Screen.fullScreenMode}",
                $"seed={SeedService.Seed} layout={(house != null ? house.LayoutHash.ToString("X8") : "n/a")}",
                $"duration_s={duration.ToString("0", c)} frames={frames.Count}",
                $"avg_fps={(1000f / avgMs).ToString("0.0", c)} avg_ms={avgMs.ToString("0.00", c)}",
                $"low1pct_fps={(1000f / low1).ToString("0.0", c)} p99_ms={p99.ToString("0.00", c)} worst_ms={worst.ToString("0.00", c)}",
            };
            var dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", PlaytestLogger.FolderName));
            Directory.CreateDirectory(dir);
            File.WriteAllLines(Path.Combine(dir, $"perf_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt"), lines);
            UnityEngine.Debug.Log("[Perf] " + string.Join(" | ", lines));
        }
    }
}
