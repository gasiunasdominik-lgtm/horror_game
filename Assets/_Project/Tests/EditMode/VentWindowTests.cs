using NUnit.Framework;
using UnityEngine;
using Vchod.Systems;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class VentWindowTests
    {
        private static readonly Vector3 Window = new(0f, 0f, 2f);
        private static readonly Vector3 Inward = Vector3.back; // window on the north wall, facing south

        [Test]
        public void InFrontAndClose_IsFreshAir()
        {
            Assert.IsTrue(VentWindow.InFreshAir(new Vector3(0.3f, 1.7f, 1f), Window, Inward, 1.6f));
        }

        [Test]
        public void TooFar_OrBehindTheWall_IsNot()
        {
            Assert.IsFalse(VentWindow.InFreshAir(new Vector3(0f, 0f, -0.5f), Window, Inward, 1.6f));
            Assert.IsFalse(VentWindow.InFreshAir(new Vector3(0f, 0f, 3f), Window, Inward, 1.6f));
        }

        [Test]
        public void AtOpenWindow_CoFallsEvenNearTheSource()
        {
            var s = new CoSettings();
            var rate = CoModel.RateFor(s, new CoExposure { AtOpenWindow = true, DistanceToSource = 1f });
            Assert.Less(rate, 0f);
        }
    }
}
