using NUnit.Framework;
using Vchod.Audio;
using Vchod.Core;

namespace Vchod.Tests
{
    public sealed class SubtitleTimingTests
    {
        [Test]
        public void LongerLinesStayUpLonger_AndNeverShorterThanMinimum()
        {
            Assert.AreEqual(SubtitleTiming.Minimum, SubtitleTiming.ReadTime("Ahoj."));
            var longLine = "Kdo je? ... Jé, ty jsi od vedle. Celý dům je dneska vzhůru nohama.";
            Assert.Greater(SubtitleTiming.ReadTime(longLine), 5.5f);
        }

        [Test]
        public void RequestedDurationIsAFloor()
        {
            Assert.AreEqual(10f, SubtitleTiming.Duration("Krátce.", 10f));
        }

        [Test]
        public void Mumble_IsAudibleAndMatchesLength()
        {
            var clip = ProceduralAudio.CreateMumble(42, 2f, drowsy: true);
            Assert.AreEqual(2f, clip.length, 0.01f);
            var data = new float[clip.samples];
            clip.GetData(data, 0);
            var peak = 0f;
            foreach (var v in data) peak = System.Math.Max(peak, System.Math.Abs(v));
            Assert.Greater(peak, 0.05f);
        }
    }
}
