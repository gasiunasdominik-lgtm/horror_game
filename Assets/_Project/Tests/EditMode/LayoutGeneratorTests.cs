using System.Collections.Generic;
using NUnit.Framework;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class LayoutGeneratorTests
    {
        private const int SweepSeeds = 200;
        private const int ReshufflesPerSeed = 5;

        private static LayoutRules Rules => new();

        [Test]
        public void SameSeed_GivesSameLayout()
        {
            var a = LayoutGenerator.Generate(12345, Rules);
            var b = LayoutGenerator.Generate(12345, Rules);
            Assert.AreEqual(a.ComputeHash(), b.ComputeHash());
            Assert.AreEqual(a.Count, b.Count);
        }

        [Test]
        public void DifferentSeeds_MostlyGiveDifferentLayouts()
        {
            var hashes = new HashSet<uint>();
            for (var seed = 1; seed <= 100; seed++) hashes.Add(LayoutGenerator.Generate(seed, Rules).ComputeHash());
            Assert.Greater(hashes.Count, 90, "layouts repeat too often; the house would not feel different");
        }

        [Test]
        public void Sweep_200Seeds_AllValid()
        {
            var rules = Rules;
            var failures = new List<string>();
            for (var seed = 1; seed <= SweepSeeds; seed++)
            {
                for (var reshuffle = 0; reshuffle < ReshufflesPerSeed; reshuffle++)
                {
                    var layoutSeed = Core.SeedService.DeriveSeed(seed, ModuleLayoutBuilder.LayoutStream, reshuffle);
                    LayoutPlan plan;
                    try
                    {
                        plan = LayoutGenerator.Generate(layoutSeed, rules);
                    }
                    catch (System.InvalidOperationException e)
                    {
                        failures.Add($"seed {seed}/{reshuffle}: {e.Message}");
                        continue;
                    }
                    var errors = LayoutValidator.Validate(plan, rules);
                    if (errors.Count > 0) failures.Add($"seed {seed}/{reshuffle}: {string.Join("; ", errors)}");
                }
            }
            Assert.IsEmpty(failures, string.Join("\n", failures));
        }

        [Test]
        public void Plan_HasPathFromStairwellToPlayerDoorAndWindow()
        {
            var plan = LayoutGenerator.Generate(7, Rules);
            var reachable = LayoutValidator.Reachable(plan);
            Assert.Contains(plan.FindFeature(CellFeature.PlayerDoor), new List<int>(reachable));
            Assert.Contains(plan.FindFeature(CellFeature.Window), new List<int>(reachable));
            Assert.AreEqual(CellFeature.PlayerDoor, plan.Cells[plan.MainPath[plan.MainPath.Count - 1]].Feature);
        }

        [Test]
        public void Stairwell_IsFixed_SoReshuffleNeverMovesItsGeometry()
        {
            for (var seed = 1; seed <= 50; seed++)
            {
                var start = LayoutGenerator.Generate(seed, Rules).Start;
                Assert.AreEqual(new GridPos(0, 0), start.Pos);
                Assert.AreEqual(1 << (int)Direction.North, start.Openings);
                Assert.AreEqual(Direction.South, start.FeatureSide);
            }
        }

        [Test]
        public void Validator_DetectsCutCorridor()
        {
            var plan = LayoutGenerator.Generate(3, Rules);
            plan.Start.Openings = 0;
            var errors = LayoutValidator.Validate(plan, Rules);
            Assert.IsNotEmpty(errors);
            Assert.That(string.Join("|", errors), Does.Contain("unreachable"));
        }

        [Test]
        public void Validator_DetectsMissingWindow()
        {
            var plan = LayoutGenerator.Generate(3, Rules);
            var w = plan.FindFeature(CellFeature.Window);
            plan.Cells[w].Feature = CellFeature.None;
            Assert.That(string.Join("|", LayoutValidator.Validate(plan, Rules)), Does.Contain("no window"));
        }

        [Test]
        public void Validator_DetectsFeatureOnOpenSide()
        {
            var plan = LayoutGenerator.Generate(3, Rules);
            var door = plan.Cells[plan.FindFeature(CellFeature.PlayerDoor)];
            door.FeatureSide = door.InDirection.Opposite(); // the side we came in through
            Assert.That(string.Join("|", LayoutValidator.Validate(plan, Rules)), Does.Contain("open side"));
        }

        [Test]
        public void EveryFeatureCell_KeepsAClosedWall()
        {
            for (var seed = 1; seed <= 50; seed++)
                foreach (var c in LayoutGenerator.Generate(seed, Rules).Cells)
                    Assert.Less(c.OpenCount, 4, $"seed {seed} cell {c.Pos} has no wall left");
        }
    }
}
