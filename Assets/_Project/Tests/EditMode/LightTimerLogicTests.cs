using NUnit.Framework;
using Vchod.Systems;

namespace Vchod.Tests
{
    public sealed class LightTimerLogicTests
    {
        [Test]
        public void StartsOff()
        {
            Assert.IsFalse(new LightTimerLogic(20f).IsOn);
        }

        [Test]
        public void Press_TurnsOnForDuration_ThenOffExactlyOnce()
        {
            var t = new LightTimerLogic(20f);
            Assert.IsTrue(t.Press());
            Assert.IsFalse(t.Tick(19.9f));
            Assert.IsTrue(t.IsOn);
            Assert.IsTrue(t.Tick(0.2f));
            Assert.IsFalse(t.IsOn);
            Assert.IsFalse(t.Tick(1f));
        }

        [Test]
        public void PressWhileOn_RestartsTimer()
        {
            var t = new LightTimerLogic(20f);
            t.Press();
            t.Tick(15f);
            Assert.IsFalse(t.Press());
            Assert.AreEqual(20f, t.Remaining, 1e-4f);
        }

        [Test]
        public void ForceOff_ReportsWhetherItWasOn()
        {
            var t = new LightTimerLogic(20f);
            Assert.IsFalse(t.ForceOff());
            t.Press();
            Assert.IsTrue(t.ForceOff());
            Assert.IsFalse(t.IsOn);
        }
    }
}
