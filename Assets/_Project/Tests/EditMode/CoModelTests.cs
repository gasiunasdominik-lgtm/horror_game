using NUnit.Framework;
using Vchod.Systems;

namespace Vchod.Tests
{
    public sealed class CoModelTests
    {
        private static CoSettings Settings => new() { startLevel = 0f };

        private static CoExposure AtSource => new() { DistanceToSource = 0f };
        private static CoExposure FarCorridor => new() { DistanceToSource = 50f };
        private static CoExposure Stairwell => new() { DistanceToSource = 50f, InStairwell = true };
        private static CoExposure Window => new() { DistanceToSource = 0f, AtOpenWindow = true };

        [Test]
        public void RisesFastestAtSource_SlowInCorridor()
        {
            var s = Settings;
            Assert.Greater(CoModel.RateFor(s, AtSource), CoModel.RateFor(s, new CoExposure { DistanceToSource = 5f }));
            Assert.Greater(CoModel.RateFor(s, new CoExposure { DistanceToSource = 5f }), CoModel.RateFor(s, FarCorridor));
            Assert.Greater(CoModel.RateFor(s, FarCorridor), 0f);
        }

        [Test]
        public void FallsInStairwell_FallsFasterAtWindow()
        {
            var s = Settings;
            Assert.Less(CoModel.RateFor(s, Stairwell), 0f);
            Assert.Less(CoModel.RateFor(s, Window), CoModel.RateFor(s, Stairwell));
        }

        [Test]
        public void WindowWinsEvenNextToSource()
        {
            Assert.Less(CoModel.RateFor(Settings, Window), 0f);
        }

        [Test]
        public void LevelIsClampedTo0And100()
        {
            var m = new CoModel(Settings);
            m.Tick(100f, Window);
            Assert.AreEqual(0f, m.Level);
            m.SetLevel(250f);
            Assert.AreEqual(100f, m.Level);
        }

        [TestCase(0f, 0)]
        [TestCase(19.9f, 0)]
        [TestCase(20f, 1)]
        [TestCase(45f, 2)]
        [TestCase(60f, 3)]
        [TestCase(85f, 4)]
        [TestCase(100f, 5)]
        public void StageMatchesGddThresholds(float level, int stage)
        {
            Assert.AreEqual(stage, CoModel.StageFor(Settings, level));
        }

        [Test]
        public void Tick_ReportsStageChangeOnce()
        {
            var m = new CoModel(Settings);
            m.SetLevel(19.5f);
            Assert.IsTrue(m.Tick(1f, AtSource));
            Assert.AreEqual(1, m.Stage);
            Assert.IsFalse(m.Tick(0.01f, FarCorridor));
        }

        [Test]
        public void StayingAtSource_LeadsToCollapse_WhichIsFinal()
        {
            var m = new CoModel(Settings);
            for (var i = 0; i < 600 && !m.IsCollapsed; i++) m.Tick(0.1f, AtSource);
            Assert.IsTrue(m.IsCollapsed);
            Assert.IsFalse(m.Tick(10f, Window));
            Assert.AreEqual(100f, m.Level);
        }

        [Test]
        public void TimeToCollapseAtSource_IsAboutFortySeconds()
        {
            var m = new CoModel(Settings);
            var t = 0f;
            while (!m.IsCollapsed && t < 300f)
            {
                m.Tick(0.1f, AtSource);
                t += 0.1f;
            }
            Assert.That(t, Is.InRange(30f, 50f));
        }

        [Test]
        public void Frozen_DoesNotChange()
        {
            var m = new CoModel(Settings) { Frozen = true };
            m.Tick(100f, AtSource);
            Assert.AreEqual(0f, m.Level);
        }

        [Test]
        public void DetectorBeepsFasterWhenCloser()
        {
            var d = new DetectorSettings();
            Assert.Less(d.IntervalFor(1f), d.IntervalFor(10f));
            Assert.Less(d.IntervalFor(10f), d.IntervalFor(20f));
            Assert.AreEqual(d.maxInterval, d.IntervalFor(1000f), 1e-4f);
            Assert.AreEqual(d.minInterval, d.IntervalFor(0f), 1e-4f);
        }
    }
}
