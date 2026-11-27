using NUnit.Framework;
using Vchod.Audio;
using Vchod.Enemy;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class LightCompromiseTests
    {
        [Test]
        public void Tube_StutersWhileStriking_ThenMostlyLit()
        {
            var offDuringStrike = 0;
            for (var t = 0f; t < TubeFlicker.StrikeTime; t += 0.01f)
                if (!TubeFlicker.IsLit(t, 1234)) offDuringStrike++;
            Assert.Greater(offDuringStrike, 5, "the tube should stutter when it strikes");

            int lit = 0, total = 0;
            for (var t = 1f; t < 61f; t += 0.01f, total++)
                if (TubeFlicker.IsLit(t, 1234)) lit++;
            Assert.That((float)lit / total, Is.InRange(0.97f, 0.9999f), "rare short dips, otherwise lit");
        }

        [Test]
        public void Tube_IsDeterministicPerLamp()
        {
            for (var t = 0f; t < 5f; t += 0.1f)
                Assert.AreEqual(TubeFlicker.IsLit(t, 42), TubeFlicker.IsLit(t, 42));
        }

        [Test]
        public void LightResponse_SplitsComeFalseAlarmLate()
        {
            Assert.AreEqual(LightReaction.Come, LightResponse.Pick(0.1, 0.6f, 0.2f));
            Assert.AreEqual(LightReaction.FalseAlarm, LightResponse.Pick(0.7, 0.6f, 0.2f));
            Assert.AreEqual(LightReaction.Late, LightResponse.Pick(0.9, 0.6f, 0.2f));
        }

        [Test]
        public void HumAndClicks_AreAudible()
        {
            foreach (var clip in new[] { ProceduralAudio.CreateTubeHum(), ProceduralAudio.CreateSwitchClick(true), ProceduralAudio.CreateSwitchClick(false) })
            {
                var data = new float[clip.samples];
                clip.GetData(data, 0);
                var peak = 0f;
                foreach (var v in data) peak = System.Math.Max(peak, System.Math.Abs(v));
                Assert.Greater(peak, 0.05f, clip.name);
            }
        }
    }
}
