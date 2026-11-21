using NUnit.Framework;
using Vchod.Enemy;

namespace Vchod.Tests
{
    public sealed class FirefighterSensesTests
    {
        [Test]
        public void InTheDark_HeSeesOnlyAlongHisTorch()
        {
            var s = new FirefighterSettings();
            s.SightFor(true, out var litRange, out var litAngle);
            s.SightFor(false, out var darkRange, out var darkAngle);
            Assert.Less(darkAngle, litAngle, "torch cone is narrower than his eyes in the light");
            Assert.Less(darkRange, litRange);
            Assert.Greater(darkRange, s.nearSenseRadius);
        }
    }
}
