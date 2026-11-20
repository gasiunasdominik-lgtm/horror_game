using UnityEngine;

namespace Vchod.Systems
{
    /// <summary>
    /// Carbon monoxide level in the player's blood, 0–100. Pure logic (no scene) so it is
    /// covered by EditMode tests. Stage = how many thresholds the level has reached (0–5).
    /// </summary>
    public sealed class CoModel
    {
        private readonly CoSettings _settings;

        public CoModel(CoSettings settings)
        {
            _settings = settings;
            Level = settings.startLevel;
            Stage = StageFor(settings, Level);
        }

        public float Level { get; private set; }
        public int Stage { get; private set; }
        /// <summary>Debug god mode: the level stops changing.</summary>
        public bool Frozen { get; set; }
        public int CollapseStage => _settings.thresholds.Length;
        public bool IsCollapsed => Stage >= CollapseStage;

        public static float RateFor(CoSettings s, CoExposure e)
        {
            if (e.AtOpenWindow) return s.windowRate;
            var proximity = Mathf.Clamp01(1f - e.DistanceToSource / s.sourceRadius);
            var fromSource = s.sourceMaxRate * proximity * proximity;
            return (e.InStairwell ? s.stairwellRate : s.corridorRate) + fromSource;
        }

        public static int StageFor(CoSettings s, float level)
        {
            var stage = 0;
            foreach (var t in s.thresholds)
                if (level >= t) stage++;
            return stage;
        }

        /// <summary>Advances the level. Returns true when the stage changed. Collapse is final.</summary>
        public bool Tick(float deltaTime, CoExposure exposure)
        {
            if (Frozen || IsCollapsed) return false;
            return SetLevel(Level + RateFor(_settings, exposure) * deltaTime);
        }

        public bool SetLevel(float level)
        {
            Level = Mathf.Clamp(level, 0f, 100f);
            var stage = StageFor(_settings, Level);
            if (stage == Stage) return false;
            Stage = stage;
            return true;
        }
    }
}
