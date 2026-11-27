using System.Collections.Generic;
using NUnit.Framework;
using Vchod.Systems;

namespace Vchod.Tests
{
    public sealed class StoryTests
    {
        [Test]
        public void Catalog_Has15To20UniquePieces_InThreeTiers_WithText()
        {
            Assert.That(StoryCatalog.All.Count, Is.InRange(15, 20));
            var ids = new HashSet<int>();
            var tiers = new HashSet<int>();
            foreach (var e in StoryCatalog.All)
            {
                Assert.IsTrue(ids.Add(e.Id), $"duplicate id {e.Id}");
                tiers.Add(e.Tier);
                Assert.IsNotEmpty(e.Title);
                Assert.IsNotEmpty(e.Text);
            }
            CollectionAssert.AreEquivalent(new[] { 1, 2, 3 }, tiers);
        }

        [Test]
        public void FirstRounds_ShowOnlyTier1_DeeperTiersOpenWithProgress()
        {
            var none = new HashSet<int>();
            Assert.AreEqual(1, StorySelection.OpenTier(none));
            foreach (var id in StorySelection.Pick(7, none, 3)) Assert.AreEqual(1, StoryCatalog.ById(id).Tier);

            var tier1 = new HashSet<int> { 1, 2, 3, 4 };
            Assert.AreEqual(2, StorySelection.OpenTier(tier1));
            var picks = StorySelection.Pick(7, tier1, 3);
            Assert.AreEqual(2, StoryCatalog.ById(picks[0]).Tier, "the newly opened tier comes first");
        }

        [Test]
        public void Pick_PrefersUncollected_AndIsDeterministic()
        {
            var have = new HashSet<int> { 1, 2 };
            var a = StorySelection.Pick(99, have, 3);
            CollectionAssert.AreEqual(a, StorySelection.Pick(99, have, 3));
            foreach (var id in a) CollectionAssert.DoesNotContain(have, id);
            CollectionAssert.AllItemsAreUnique(a);
        }

        [Test]
        public void Progress_RoundTrips_AndIgnoresUnknownIds()
        {
            var set = StoryProgress.Parse("3,5,999,x");
            CollectionAssert.AreEquivalent(new[] { 3, 5 }, set);
            CollectionAssert.AreEquivalent(set, StoryProgress.Parse(StoryProgress.Serialize(set)));
        }

        [Test]
        public void EveryFloor_GetsAPiece_WhenAllCollected()
        {
            var all = new HashSet<int>();
            foreach (var e in StoryCatalog.All) all.Add(e.Id);
            Assert.AreEqual(3, StorySelection.Pick(1, all, 3).Count, "re-reading is fine once everything is found");
        }
    }
}
