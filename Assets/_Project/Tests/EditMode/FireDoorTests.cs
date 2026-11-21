using NUnit.Framework;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class FireDoorTests
    {
        [Test]
        public void EveryFloor_HasOneFireDoor_FarFromTheStairs_AndValid()
        {
            var rules = new LayoutRules();
            foreach (var playerFloor in new[] { true, false })
            for (var seed = 1; seed <= 200; seed++)
            {
                var plan = LayoutGenerator.Generate(seed, rules, playerFloor);
                CollectionAssert.IsEmpty(LayoutValidator.Validate(plan, rules), $"seed {seed}");
                var maxDepth = 0;
                var doorDepth = -1;
                for (var i = 0; i < plan.Count; i++)
                {
                    var c = plan.Cells[i];
                    if (c.Feature != CellFeature.PlayerDoor && c.Feature != CellFeature.Elevator && c.Depth > maxDepth) maxDepth = c.Depth;
                    if (c.HasFireDoor) doorDepth = c.Depth;
                }
                Assert.GreaterOrEqual(doorDepth, maxDepth - 1, $"seed {seed}: fire door should be at the far end");
            }
        }
    }
}
