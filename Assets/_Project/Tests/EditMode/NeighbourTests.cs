using NUnit.Framework;
using UnityEngine;
using Vchod.Systems;
using Vchod.World;

namespace Vchod.Tests
{
    public sealed class NeighbourTests
    {
        [Test]
        public void Floor_IsSeeded_AndSometimesTrapSometimesHelp()
        {
            int trap = 0, help = 0;
            for (var seed = 1; seed <= 400; seed++)
            {
                var f = NeighbourPlacement.FloorFor(seed, playerFloor: 2, trapChance: 0.4f);
                Assert.AreEqual(f, NeighbourPlacement.FloorFor(seed, 2, 0.4f));
                Assert.That(f, Is.InRange(0, 2));
                if (f == 2) trap++;
                else help++;
            }
            Assert.That(trap / 400f, Is.InRange(0.3f, 0.5f));
            Assert.Greater(help, 0);
        }

        [Test]
        public void OnThePlayersFloor_SheLivesAtTheCoSource()
        {
            var plan = LayoutGenerator.Generate(9, new LayoutRules(), playerFloor: true);
            Assert.AreEqual(plan.CoSourceIndex, NeighbourPlacement.DoorCell(plan, 9, 2, 0));
        }

        [Test]
        public void ElsewhereSheIsBehindAnApartmentDoor()
        {
            for (var seed = 1; seed <= 50; seed++)
            {
                var plan = LayoutGenerator.Generate(seed, new LayoutRules(), playerFloor: false);
                var cell = NeighbourPlacement.DoorCell(plan, seed, 0, 0);
                Assert.AreEqual(CellFeature.ApartmentDoor, plan.Cells[cell].Feature, $"seed {seed}");
            }
        }

        [Test]
        public void EnteringTheTrapFlat_IsAnInstantCollapse()
        {
            var model = new CoModel(new CoSettings());
            model.SetLevel(100f);
            Assert.IsTrue(model.IsCollapsed);
        }
    }
}
