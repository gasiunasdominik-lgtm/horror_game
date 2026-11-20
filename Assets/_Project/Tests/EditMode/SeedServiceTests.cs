using NUnit.Framework;
using Vchod.Core;

namespace Vchod.Tests
{
    public sealed class SeedServiceTests
    {
        [Test]
        public void TryParseSeedArgument_ReadsValueAfterFlag()
        {
            Assert.IsTrue(SeedService.TryParseSeedArgument(new[] { "Vchod.exe", "-seed", "1234" }, out var seed));
            Assert.AreEqual(1234, seed);
        }

        [Test]
        public void TryParseSeedArgument_IsCaseInsensitive()
        {
            Assert.IsTrue(SeedService.TryParseSeedArgument(new[] { "-SEED", "7" }, out var seed));
            Assert.AreEqual(7, seed);
        }

        private static readonly string[][] InvalidArgs =
        {
            new string[0],
            new[] { "-seed" },
            new[] { "-seed", "abc" },
            new[] { "-other", "5" },
        };

        [TestCaseSource(nameof(InvalidArgs))]
        public void TryParseSeedArgument_RejectsMissingOrInvalid(string[] args)
        {
            Assert.IsFalse(SeedService.TryParseSeedArgument(args, out _));
        }

        [Test]
        public void TryParseSeedArgument_HandlesNull()
        {
            Assert.IsFalse(SeedService.TryParseSeedArgument(null, out _));
        }

        [Test]
        public void DeriveSeed_IsDeterministic()
        {
            Assert.AreEqual(SeedService.DeriveSeed(42, "layout", 3), SeedService.DeriveSeed(42, "layout", 3));
        }

        [Test]
        public void DeriveSeed_IsStableAcrossVersions()
        {
            // Pinned value: if this changes, every recorded seed in BUGS.md stops reproducing.
            Assert.AreEqual(PinnedLayoutSeed, SeedService.DeriveSeed(1, "layout", 0));
        }

        [Test]
        public void DeriveSeed_DiffersPerStreamIndexAndSeed()
        {
            var a = SeedService.DeriveSeed(42, "layout", 0);
            Assert.AreNotEqual(a, SeedService.DeriveSeed(42, "layout", 1));
            Assert.AreNotEqual(a, SeedService.DeriveSeed(42, "neighbors", 0));
            Assert.AreNotEqual(a, SeedService.DeriveSeed(43, "layout", 0));
        }

        [Test]
        public void DeriveSeed_IsNonNegative()
        {
            for (var i = 0; i < 1000; i++)
                Assert.GreaterOrEqual(SeedService.DeriveSeed(i * 7919, "x", i), 0);
        }

        [Test]
        public void CreateStream_SameSeedGivesSameSequence()
        {
            SeedService.Initialize(99);
            var a = SeedService.CreateStream("test");
            var b = SeedService.CreateStream("test");
            for (var i = 0; i < 10; i++) Assert.AreEqual(a.Next(), b.Next());
        }

        [Test]
        public void NewRandomSeed_IsInReadableRange()
        {
            for (var i = 0; i < 200; i++)
            {
                var seed = SeedService.NewRandomSeed();
                Assert.That(seed, Is.InRange(1, SeedService.MaxRandomSeed));
            }
        }

        private const int PinnedLayoutSeed = 777766942;
    }
}
