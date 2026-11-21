using NUnit.Framework;
using UnityEngine;
using Vchod.Audio;

namespace Vchod.Tests
{
    /// <summary>Placeholder sounds must exist and be audible, or the player loses the firefighter's position cue.</summary>
    public sealed class ProceduralAudioTests
    {
        private static float Peak(AudioClip clip)
        {
            var data = new float[clip.samples];
            clip.GetData(data, 0);
            var peak = 0f;
            foreach (var v in data) peak = Mathf.Max(peak, Mathf.Abs(v));
            Object.DestroyImmediate(clip);
            return peak;
        }

        [Test] public void MaskBreath_IsAudible() => Assert.That(Peak(ProceduralAudio.CreateMaskBreath()), Is.InRange(0.1f, 1f));
        [Test] public void GearStep_IsAudible() => Assert.That(Peak(ProceduralAudio.CreateGearStep(1)), Is.InRange(0.1f, 1f));
        [Test] public void MaskedCall_IsAudible() => Assert.That(Peak(ProceduralAudio.CreateMaskedCall()), Is.InRange(0.1f, 1f));
        [Test] public void Beep_IsAudible() => Assert.That(Peak(ProceduralAudio.CreateBeep(3100f, 0.09f)), Is.InRange(0.1f, 1f));

        [Test]
        public void MaskBreath_IsDeterministic()
        {
            var a = ProceduralAudio.CreateMaskBreath();
            var b = ProceduralAudio.CreateMaskBreath();
            var da = new float[a.samples];
            var db = new float[b.samples];
            a.GetData(da, 0);
            b.GetData(db, 0);
            Assert.AreEqual(da, db);
            Object.DestroyImmediate(a);
            Object.DestroyImmediate(b);
        }
    }
}
