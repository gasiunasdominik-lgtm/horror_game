using NUnit.Framework;
using UnityEngine;
using Vchod.Enemy;

namespace Vchod.Tests
{
    public sealed class ArrivalTests
    {
        [Test]
        public void PicksTheEntryFarthestFromThePlayer()
        {
            var entries = new[] { new Vector3(2, 0, 0), new Vector3(20, 0, 0) };
            Assert.IsTrue(ArrivalPoint.Pick(entries, Vector3.zero, 10f, out var p));
            Assert.AreEqual(entries[1], p);
        }

        [Test]
        public void NeverArrivesCloserThanTheMinimum()
        {
            var entries = new[] { new Vector3(3, 0, 0), new Vector3(0, 0, 8) };
            Assert.IsFalse(ArrivalPoint.Pick(entries, Vector3.zero, 10f, out _));
        }

        [Test]
        public void DefaultsGiveACalmStartAndRareRandomChecks()
        {
            var s = new FirefighterSettings();
            Assert.GreaterOrEqual(s.introGrace, 60f);
            Assert.LessOrEqual(s.hidingCheckChance, 0.25f);
        }
    }
}
