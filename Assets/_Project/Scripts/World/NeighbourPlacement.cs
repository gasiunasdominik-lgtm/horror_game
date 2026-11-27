using System.Collections.Generic;
using Vchod.Core;

namespace Vchod.World
{
    /// <summary>
    /// Where neighbour 1 (the cake lady) lives this round. On the player's floor she is the flat at
    /// the CO source (a trap); lower down she is a real help (a shortcut up). Pure and seeded.
    /// </summary>
    public static class NeighbourPlacement
    {
        public const string Stream = "neighbour1";

        public static int FloorFor(int roundSeed, int playerFloor, float trapChance)
        {
            var rng = new System.Random(SeedService.DeriveSeed(roundSeed, Stream, 0));
            if (playerFloor <= 0 || rng.NextDouble() < trapChance) return playerFloor;
            return rng.Next(0, playerFloor);
        }

        public const string RadioStream = "neighbour2";
        public const string ArgumentStream = "neighbour3";

        /// <summary>Neighbours 2 and 3 live on any floor; the stream keeps them independent.</summary>
        public static int FloorForOther(int roundSeed, string stream, int floorCount)
        {
            var rng = new System.Random(SeedService.DeriveSeed(roundSeed, stream, 0));
            return rng.Next(0, floorCount);
        }

        /// <summary>A seeded apartment door that is not the CO source and not one of the taken cells.</summary>
        public static int OtherDoorCell(LayoutPlan plan, int roundSeed, string stream, int floor, int reshuffleIndex, params int[] taken)
        {
            var doors = new List<int>();
            for (var i = 0; i < plan.Count; i++)
            {
                var c = plan.Cells[i];
                if (c.Feature != CellFeature.ApartmentDoor || c.IsCoSource) continue;
                if (System.Array.IndexOf(taken, i) >= 0) continue;
                doors.Add(i);
            }
            if (doors.Count == 0) return -1;
            var rng = new System.Random(SeedService.DeriveSeed(roundSeed, $"{stream}/door{floor}", reshuffleIndex));
            return doors[rng.Next(doors.Count)];
        }

        /// <summary>Her door cell on this layout: the CO source on the player's floor, else a seeded apartment door.</summary>
        public static int DoorCell(LayoutPlan plan, int roundSeed, int floor, int reshuffleIndex)
        {
            if (plan.IsPlayerFloor) return plan.CoSourceIndex;
            var doors = new List<int>();
            for (var i = 0; i < plan.Count; i++)
                if (plan.Cells[i].Feature == CellFeature.ApartmentDoor) doors.Add(i);
            if (doors.Count == 0) return -1;
            var rng = new System.Random(SeedService.DeriveSeed(roundSeed, $"{Stream}/door{floor}", reshuffleIndex));
            return doors[rng.Next(doors.Count)];
        }
    }
}
