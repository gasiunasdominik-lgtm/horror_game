using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Vchod.Enemy;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class HidingTests
    {
        private static readonly List<Vector3> Spots = new() { new(0, 0, 0), new(5, 0, 0), new(20, 0, 0) };
        private static readonly List<int> None = new();

        [Test]
        public void SawPlayerHide_ThatSpotIsAlwaysCheckedFirst()
        {
            Assert.AreEqual(2, HidingSearch.Pick(Spots, Vector3.zero, 8f, None, sawHide: 2, roll: 0.99, chance: 0f, out _));
        }

        [Test]
        public void LuckyRoll_ChecksNearestUncheckedInRadius()
        {
            Assert.AreEqual(0, HidingSearch.Pick(Spots, new Vector3(1, 0, 0), 8f, None, -1, roll: 0.1, chance: 0.5f, out _));
            Assert.AreEqual(1, HidingSearch.Pick(Spots, new Vector3(1, 0, 0), 8f, new List<int> { 0 }, -1, 0.1, 0.5f, out _));
        }

        [Test]
        public void UnluckyRoll_OrNothingInRadius_KeepsWandering()
        {
            Assert.AreEqual(-1, HidingSearch.Pick(Spots, Vector3.zero, 8f, None, -1, roll: 0.7, chance: 0.5f, out var passed));
            Assert.AreEqual(0, passed, "the spot he walked past is reported, so it is not rolled again");
            Assert.AreEqual(-1, HidingSearch.Pick(Spots, new Vector3(12, 0, 0), 4f, None, -1, roll: 0.1, chance: 0.5f, out _));
        }

        [Test]
        public void CatchHidden_RespectsGodMode()
        {
            var b = new FirefighterBrain(new FirefighterSettings()) { CanCatch = false };
            Assert.IsFalse(b.CatchHidden());
            b.CanCatch = true;
            Assert.IsTrue(b.CatchHidden());
            Assert.IsTrue(b.HasCaughtPlayer);
        }

        [Test]
        public void EveryFloor_HasHidingSpots_OnFreeWalls_AndOneOnTheWay()
        {
            var rules = new LayoutRules();
            for (var seed = 1; seed <= 200; seed++)
            {
                var plan = LayoutGenerator.Generate(seed, rules);
                var count = 0;
                var onMain = false;
                foreach (var c in plan.Cells)
                {
                    if (c.Hiding == HidingKind.None) continue;
                    count++;
                    onMain |= c.MainPath;
                    Assert.IsFalse(c.IsOpen(c.HidingSide), $"seed {seed}");
                    Assert.IsFalse(c.Feature != CellFeature.None && c.FeatureSide == c.HidingSide, $"seed {seed}");
                }
                Assert.That(count, Is.InRange(rules.minHidingSpots, rules.maxHidingSpots), $"seed {seed}");
                Assert.IsTrue(onMain, $"seed {seed}");
            }
        }
    }
}
