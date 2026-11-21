using NUnit.Framework;
using Vchod.Enemy;

namespace Vchod.Tests
{
    public sealed class PatrolActivityTests
    {
        [Test]
        public void HighRoll_WalksOn() => Assert.AreEqual(PatrolAction.None, PatrolActivity.Pick(0.9, true, 0.4f, 0.5f));

        [Test]
        public void LowRoll_KnocksWhenADoorIsNear_ElseLooksAround()
        {
            Assert.AreEqual(PatrolAction.Knock, PatrolActivity.Pick(0.1, true, 0.4f, 0.5f));
            Assert.AreEqual(PatrolAction.LookAround, PatrolActivity.Pick(0.1, false, 0.4f, 0.5f));
            Assert.AreEqual(PatrolAction.LookAround, PatrolActivity.Pick(0.3, true, 0.4f, 0.5f));
        }

        [Test]
        public void Defaults_UsuallyLeaveAfterAFruitlessSearch()
        {
            var s = new FirefighterSettings();
            Assert.GreaterOrEqual(s.leaveAfterSearch, 0.5f);
            Assert.Greater(s.patrolStayMax, s.patrolStayMin);
        }
    }
}
