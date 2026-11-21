using NUnit.Framework;
using Vchod.Core;
using Vchod.Systems;

namespace Vchod.Tests
{
    public sealed class EndingTests
    {
        [Test]
        public void CaughtAtOpenWindow_IsFreshAir_ElsewhereEvacuation()
        {
            Assert.AreEqual(EndingId.FreshAir, EndingRules.ForCatch(atOpenWindow: true));
            Assert.AreEqual(EndingId.Evacuation, EndingRules.ForCatch(atOpenWindow: false));
        }

        [Test]
        public void EveryEnding_HasTitleAndBody_ForEveryCause()
        {
            foreach (var e in new[] { EndingId.Collapse, EndingId.Evacuation, EndingId.FreshAir })
            {
                Assert.IsNotEmpty(EndingTexts.Title(e));
                foreach (CollapseCause c in System.Enum.GetValues(typeof(CollapseCause)))
                    Assert.IsNotEmpty(EndingTexts.Body(e, c));
            }
        }

        [Test]
        public void CollapseText_DependsOnWhereItHappened()
        {
            Assert.AreNotEqual(EndingTexts.Body(EndingId.Collapse, CollapseCause.Home),
                EndingTexts.Body(EndingId.Collapse, CollapseCause.Corridor));
        }
    }
}
