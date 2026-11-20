using NUnit.Framework;
using Vchod.Systems;

namespace Vchod.Tests
{
    public sealed class SymptomStateTests
    {
        private static readonly SymptomSettings S = new();

        private static SymptomState At(float level, bool collapsed = false) =>
            SymptomState.Evaluate(S, level, collapsed, 0.5f);

        [Test]
        public void CleanAir_HasNoSymptoms()
        {
            var s = At(5f);
            Assert.AreEqual(0f, s.HumVolume, 1e-4f);
            Assert.AreEqual(S.baseVignette, s.Vignette, 1e-4f);
            Assert.AreEqual(1f, s.LookMultiplier, 1e-4f);
            Assert.AreEqual(1f, s.SpeedMultiplier, 1e-4f);
            Assert.AreEqual(SymptomState.OpenCutoff, s.LowPassCutoff, 1f);
            Assert.AreEqual(0f, s.PostExposure, 1e-4f);
        }

        [Test]
        public void At20_EarsRing()
        {
            Assert.Greater(At(20f).HumVolume, 0f);
            Assert.AreEqual(1f, At(20f).LookMultiplier, 1e-4f);
        }

        [Test]
        public void At40_ViewNarrowsAndTurningSlows()
        {
            var s = At(40f);
            Assert.AreEqual(S.narrowVignette, s.Vignette, 1e-4f);
            Assert.AreEqual(S.narrowLookMultiplier, s.LookMultiplier, 1e-4f);
            Assert.AreEqual(1f, s.SpeedMultiplier, 1e-4f);
        }

        [Test]
        public void At60_SoundMuffledAndGeometryWarps()
        {
            var s = At(60f);
            Assert.AreEqual(S.muffledCutoff, s.LowPassCutoff, 1f);
            Assert.AreEqual(S.warpChromatic, s.ChromaticAberration, 1e-4f);
            Assert.AreEqual(0f, At(45f).ChromaticAberration, 1e-4f);
        }

        [Test]
        public void At80_NearlyDarkAndWalking30PercentSlower()
        {
            var s = At(80f);
            Assert.AreEqual(0.7f, s.SpeedMultiplier, 1e-4f);
            Assert.Less(s.PostExposure, -1f);
        }

        [Test]
        public void Collapse_BlacksOutAndStops()
        {
            var s = At(100f, collapsed: true);
            Assert.AreEqual(0f, s.SpeedMultiplier);
            Assert.LessOrEqual(s.PostExposure, S.collapsePostExposure);
        }

        [Test]
        public void SymptomsNeverDecreaseAsCoRises()
        {
            var prev = At(0f);
            for (var level = 1f; level <= 100f; level += 1f)
            {
                var s = At(level);
                Assert.GreaterOrEqual(s.Vignette, prev.Vignette - 1e-5f, $"vignette at {level}");
                Assert.LessOrEqual(s.SpeedMultiplier, prev.SpeedMultiplier + 1e-5f, $"speed at {level}");
                Assert.LessOrEqual(s.LookMultiplier, prev.LookMultiplier + 1e-5f, $"look at {level}");
                Assert.LessOrEqual(s.LowPassCutoff, prev.LowPassCutoff + 1e-3f, $"cutoff at {level}");
                prev = s;
            }
        }
    }
}
