using NUnit.Framework;
using Vchod.Enemy;

namespace Vchod.Tests
{
    public sealed class FloorRoamTests
    {
        [Test]
        public void LowRollMovesTowardThePlayer()
        {
            Assert.AreEqual(1, FloorRoam.Step(2, 0, 3, 0.1, 0.6f));
            Assert.AreEqual(1, FloorRoam.Step(0, 2, 3, 0.1, 0.6f));
        }

        [Test]
        public void HighRollMovesAway_OrTowardAtTheEdge()
        {
            Assert.AreEqual(2, FloorRoam.Step(1, 0, 3, 0.9, 0.6f));
            Assert.AreEqual(1, FloorRoam.Step(2, 0, 3, 0.9, 0.6f)); // top floor: can only go down
        }

        [Test]
        public void AlwaysStaysInsideTheHouse()
        {
            for (var cur = 0; cur < 3; cur++)
            for (var pl = 0; pl < 3; pl++)
            for (var r = 0.0; r < 1.0; r += 0.1)
            {
                var next = FloorRoam.Step(cur, pl, 3, r, 0.6f);
                Assert.That(next, Is.InRange(0, 2));
            }
        }
    }
}
