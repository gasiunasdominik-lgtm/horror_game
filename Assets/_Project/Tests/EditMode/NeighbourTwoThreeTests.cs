using NUnit.Framework;
using UnityEngine;
using Vchod.Audio;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class NeighbourTwoThreeTests
    {
        [Test]
        public void RadioMan_SaysWhereTheMaskedManIs()
        {
            StringAssert.Contains("Nahoře", RadioNeighbour.MaskLine(2, 0));
            StringAssert.Contains("Dole", RadioNeighbour.MaskLine(0, 2));
            StringAssert.Contains("Před chvílí", RadioNeighbour.MaskLine(1, 1));
            Assert.IsNotEmpty(RadioNeighbour.MaskLine(-1, 1));
        }

        [Test]
        public void Neighbours2And3_NeverTakeTheCoSourceOrTheSameDoor()
        {
            var rules = new LayoutRules();
            for (var seed = 1; seed <= 200; seed++)
            {
                var plan = LayoutGenerator.Generate(seed, rules, playerFloor: true);
                var radio = NeighbourPlacement.OtherDoorCell(plan, seed, NeighbourPlacement.RadioStream, 2, 0, plan.CoSourceIndex);
                var tv = NeighbourPlacement.OtherDoorCell(plan, seed, NeighbourPlacement.ArgumentStream, 2, 0, plan.CoSourceIndex, radio);
                if (radio >= 0) Assert.IsFalse(plan.Cells[radio].IsCoSource, $"seed {seed}");
                if (tv >= 0)
                {
                    Assert.IsFalse(plan.Cells[tv].IsCoSource, $"seed {seed}");
                    Assert.AreNotEqual(radio, tv, $"seed {seed}");
                }
            }
        }

        [Test]
        public void Argument_IsAFixedLoop_AndRadioIsAudible()
        {
            var a = ProceduralAudio.CreateArgument();
            var b = ProceduralAudio.CreateArgument();
            var da = new float[a.samples];
            var db = new float[b.samples];
            a.GetData(da, 0);
            b.GetData(db, 0);
            CollectionAssert.AreEqual(da, db, "the same fight word for word: a TV");
            var r = ProceduralAudio.CreateRadio();
            var dr = new float[r.samples];
            r.GetData(dr, 0);
            var peak = 0f;
            foreach (var v in dr) peak = Mathf.Max(peak, Mathf.Abs(v));
            Assert.Greater(peak, 0.03f);
        }

        [Test]
        public void NoiseMask_CoversStepsNearby()
        {
            var go = new GameObject("tv");
            var mask = go.AddComponent<NoiseMask>();
            mask.Radius = 5f;
            mask.Factor = 0.35f;
            try
            {
                Assert.AreEqual(0.35f, NoiseMask.FactorAt(new Vector3(2, 0, 0)), 1e-4f);
                Assert.AreEqual(1f, NoiseMask.FactorAt(new Vector3(9, 0, 0)), 1e-4f);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
