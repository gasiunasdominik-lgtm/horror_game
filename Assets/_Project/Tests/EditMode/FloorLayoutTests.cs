using NUnit.Framework;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class FloorLayoutTests
    {
        [Test]
        public void OtherFloors_HaveNoPlayerDoorAndNoCoSource_ButStayValid()
        {
            var rules = new LayoutRules();
            for (var seed = 1; seed <= 200; seed++)
            {
                var plan = LayoutGenerator.Generate(seed, rules, playerFloor: false);
                Assert.IsFalse(plan.IsPlayerFloor);
                Assert.AreEqual(-1, plan.FindFeature(CellFeature.PlayerDoor), $"seed {seed}");
                Assert.AreEqual(-1, plan.CoSourceIndex, $"seed {seed}");
                Assert.AreEqual(CellFeature.ApartmentDoor, plan.Cells[plan.MainPath[plan.MainPath.Count - 1]].Feature, "dead end is a neighbour's door");
                CollectionAssert.IsEmpty(LayoutValidator.Validate(plan, rules), $"seed {seed}");
            }
        }

        [Test]
        public void PlayerFloor_StillHasExactlyOneDoorAndSource()
        {
            var plan = LayoutGenerator.Generate(5, new LayoutRules(), playerFloor: true);
            Assert.GreaterOrEqual(plan.FindFeature(CellFeature.PlayerDoor), 0);
            Assert.GreaterOrEqual(plan.CoSourceIndex, 0);
        }

        [Test]
        public void EveryFloor_HasTheLiftInTheFirstCorridorCell_OnAClosedWall()
        {
            foreach (var playerFloor in new[] { true, false })
            for (var seed = 1; seed <= 200; seed++)
            {
                var plan = LayoutGenerator.Generate(seed, new LayoutRules(), playerFloor);
                var lift = plan.Cells[plan.MainPath[1]];
                Assert.AreEqual(CellFeature.Elevator, lift.Feature, $"seed {seed}");
                Assert.IsFalse(lift.IsOpen(lift.FeatureSide), $"seed {seed}");
            }
        }
    }
}
