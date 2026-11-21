using NUnit.Framework;
using Vchod.World;

namespace Vchod.Tests
{
    /// <summary>Playtest #1 (D-030): a switch must always be close.</summary>
    public sealed class LightSwitchLayoutTests
    {
        [Test]
        public void EveryCell_IsAtMostTwoCellsFromASwitch_AndSwitchesAreSpread()
        {
            var rules = new LayoutRules();
            for (var seed = 1; seed <= 200; seed++)
            {
                var plan = LayoutGenerator.Generate(seed, rules);
                var switches = 0;
                for (var i = 0; i < plan.Count; i++)
                {
                    Assert.IsTrue(LayoutValidator.SwitchDistance(plan, i, 2), $"seed {seed} cell {i}");
                    if (plan.Cells[i].HasSwitch) switches++;
                }
                Assert.LessOrEqual(switches, (plan.Count + 1) / 2, $"seed {seed}: at most every second cell");
                Assert.Greater(switches, 0, $"seed {seed}");
            }
        }
    }
}
