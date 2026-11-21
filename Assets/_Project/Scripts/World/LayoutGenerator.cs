using System;
using System.Collections.Generic;

namespace Vchod.World
{
    /// <summary>
    /// Builds a floor as a corridor tree: stairwell → winding main path → player's door,
    /// plus short side branches. Same seed + rules = same plan (only System.Random(seed) is used).
    /// </summary>
    public static class LayoutGenerator
    {
        private const int StraightWeight = 3;
        private const int TurnWeight = 1;

        public static LayoutPlan Generate(int seed, LayoutRules rules, bool playerFloor = true)
        {
            var rng = new Random(seed);
            for (var attempt = 0; attempt < rules.maxAttempts; attempt++)
            {
                var plan = TryGenerate(rng, rules, playerFloor);
                if (plan == null) continue;
                plan.Seed = seed;
                if (LayoutValidator.Validate(plan, rules).Count == 0) return plan;
            }
            throw new InvalidOperationException($"No valid layout for seed {seed} in {rules.maxAttempts} attempts");
        }

        private static LayoutPlan TryGenerate(Random rng, LayoutRules rules, bool playerFloor)
        {
            var plan = new LayoutPlan { IsPlayerFloor = playerFloor };
            plan.Add(new LayoutCell
            {
                Pos = new GridPos(0, 0),
                InDirection = Direction.North,
                MainPath = true,
                Feature = CellFeature.Stairwell,
                FeatureSide = Direction.South,
            });
            plan.MainPath.Add(LayoutPlan.StartIndex);

            // Main path. The first step always leaves the stairwell to the north.
            var length = rng.Next(rules.mainPathMin, rules.mainPathMax + 1);
            var current = LayoutPlan.StartIndex;
            for (var i = 0; i < length; i++)
            {
                var dir = i == 0 ? Direction.North : PickDirection(rng, plan, rules, current);
                if (dir == null || !CanPlace(plan, rules, plan.Cells[current].Pos + dir.Value.Offset())) return null;
                current = Extend(plan, current, dir.Value, mainPath: true);
                plan.MainPath.Add(current);
            }

            var door = plan.Cells[current];
            // Other floors end in a neighbour's door: same dead end, different door.
            door.Feature = playerFloor ? CellFeature.PlayerDoor : CellFeature.ApartmentDoor;
            door.FeatureSide = door.InDirection; // dead end: the wall straight ahead

            // The lift faces the stairs, in the first corridor cell; branches must not open its wall.
            AssignFeature(rng, plan.Cells[plan.MainPath[1]], CellFeature.Elevator);

            // Side branches off the main path (never from the stairwell or the door cell).
            var branchEnds = new List<int>();
            for (var b = 0; b < rules.branchCount; b++)
            {
                var from = plan.MainPath[rng.Next(1, plan.MainPath.Count - 1)];
                var branchLength = rng.Next(1, rules.branchMaxLength + 1);
                var tip = -1;
                for (var step = 0; step < branchLength; step++)
                {
                    var origin = tip < 0 ? from : tip;
                    var dir = PickDirection(rng, plan, rules, origin);
                    if (dir == null) break;
                    tip = Extend(plan, origin, dir.Value, mainPath: false);
                }
                if (tip >= 0) branchEnds.Add(tip);
            }

            // A window: prefer the end of a side branch, so fresh air is a detour, not the route.
            var windowCell = branchEnds.Count > 0
                ? branchEnds[rng.Next(branchEnds.Count)]
                : PickCellWithClosedSide(rng, plan, plan.MainPath.GetRange(1, plan.MainPath.Count - 2));
            if (windowCell < 0) return null;
            AssignFeature(rng, plan.Cells[windowCell], CellFeature.Window);

            // Apartment doors on remaining cells, topped up to the minimum.
            var free = new List<int>();
            for (var i = 1; i < plan.Count; i++)
            {
                var c = plan.Cells[i];
                if (c.Feature == CellFeature.None && c.OpenCount < 4) free.Add(i);
            }
            Shuffle(rng, free);
            var doors = new List<int>();
            foreach (var i in free)
                if (rng.NextDouble() < rules.apartmentDoorChance) doors.Add(i);
            foreach (var i in free)
            {
                if (doors.Count >= rules.minApartmentDoors) break;
                if (!doors.Contains(i)) doors.Add(i);
            }
            if (doors.Count < rules.minApartmentDoors) return null;
            foreach (var i in doors) AssignFeature(rng, plan.Cells[i], CellFeature.ApartmentDoor);

            // The CO source is one neighbour's flat on the player's floor; avoid the cell right next to the stairs if possible.
            if (playerFloor)
            {
                var candidates = doors.FindAll(i => plan.Cells[i].Depth >= 2);
                if (candidates.Count == 0) candidates = doors;
                plan.Cells[candidates[rng.Next(candidates.Count)]].IsCoSource = true;
            }

            PlaceHidingSpots(rng, plan, rules);
            PlaceLightSwitches(rng, plan);
            return plan;
        }

        private static int Extend(LayoutPlan plan, int from, Direction dir, bool mainPath)
        {
            var parent = plan.Cells[from];
            var cell = new LayoutCell
            {
                Pos = parent.Pos + dir.Offset(),
                Parent = from,
                InDirection = dir,
                Depth = parent.Depth + 1,
                MainPath = mainPath,
            };
            parent.Open(dir);
            cell.Open(dir.Opposite());
            return plan.Add(cell);
        }

        private static bool CanPlace(LayoutPlan plan, LayoutRules rules, GridPos pos) =>
            !plan.IsOccupied(pos) && pos.ChebyshevLength <= rules.gridRadius;

        /// <summary>Weighted random free direction; going straight is likelier than turning. Null if boxed in.</summary>
        private static Direction? PickDirection(Random rng, LayoutPlan plan, LayoutRules rules, int from)
        {
            var cell = plan.Cells[from];
            Span<int> weights = stackalloc int[4];
            var total = 0;
            foreach (var d in DirectionUtil.All)
            {
                if (d == cell.InDirection.Opposite() || cell.IsOpen(d) || !CanPlace(plan, rules, cell.Pos + d.Offset()))
                    continue;
                if (cell.Feature != CellFeature.None && d == cell.FeatureSide) continue;
                // A cell must keep one closed wall for a door or window, so never open the 4th side.
                if (cell.OpenCount >= 3) continue;
                var w = d == cell.InDirection ? StraightWeight : TurnWeight;
                weights[(int)d] = w;
                total += w;
            }
            if (total == 0) return null;

            var roll = rng.Next(total);
            foreach (var d in DirectionUtil.All)
            {
                roll -= weights[(int)d];
                if (roll < 0) return d;
            }
            return null;
        }

        private static void AssignFeature(Random rng, LayoutCell cell, CellFeature feature)
        {
            cell.Feature = feature;
            // Prefer the wall ahead (most visible when walking in), else a random closed side.
            if (!cell.IsOpen(cell.InDirection))
            {
                cell.FeatureSide = cell.InDirection;
                return;
            }
            var closed = new List<Direction>(3);
            foreach (var d in DirectionUtil.All)
                if (!cell.IsOpen(d)) closed.Add(d);
            cell.FeatureSide = closed[rng.Next(closed.Count)];
        }

        /// <summary>
        /// Wardrobes and tables on free walls. At least one on the main path, so a hiding
        /// spot is always on the way, never only in a side corridor.
        /// </summary>
        private static void PlaceHidingSpots(Random rng, LayoutPlan plan, LayoutRules rules)
        {
            var free = new List<int>();
            for (var i = 1; i < plan.Count; i++)
                if (plan.Cells[i].Feature != CellFeature.PlayerDoor && FreeSides(plan.Cells[i]).Count > 0) free.Add(i);
            Shuffle(rng, free);
            var main = free.FindIndex(i => plan.Cells[i].MainPath);
            if (main > 0) (free[0], free[main]) = (free[main], free[0]);

            var count = Math.Min(free.Count, rng.Next(rules.minHidingSpots, rules.maxHidingSpots + 1));
            for (var k = 0; k < count; k++)
            {
                var cell = plan.Cells[free[k]];
                var sides = FreeSides(cell);
                cell.HidingSide = sides[rng.Next(sides.Count)];
                cell.Hiding = rng.NextDouble() < rules.wardrobeChance ? HidingKind.Wardrobe : HidingKind.Table;
            }
        }

        /// <summary>
        /// Playtest #1: one switch at the stairs was too far. Walking outward by depth, a cell gets a
        /// switch unless it or a neighbour already has one, so a switch is never more than 2 cells away.
        /// </summary>
        private static void PlaceLightSwitches(Random rng, LayoutPlan plan)
        {
            var order = new List<int>();
            for (var i = 1; i < plan.Count; i++) order.Add(i);
            order.Sort((a, b) => plan.Cells[a].Depth.CompareTo(plan.Cells[b].Depth));
            foreach (var i in order)
            {
                if (LayoutValidator.SwitchDistance(plan, i, 1)) continue;
                var cell = plan.Cells[i];
                var sides = FreeSides(cell);
                if (cell.Hiding != HidingKind.None) sides.Remove(cell.HidingSide);
                if (sides.Count == 0) continue;
                cell.HasSwitch = true;
                cell.SwitchSide = sides[rng.Next(sides.Count)];
            }
        }

        /// <summary>Closed walls not used by the cell's door or window.</summary>
        internal static List<Direction> FreeSides(LayoutCell cell)
        {
            var sides = new List<Direction>(3);
            foreach (var d in DirectionUtil.All)
                if (!cell.IsOpen(d) && (cell.Feature == CellFeature.None || d != cell.FeatureSide)) sides.Add(d);
            return sides;
        }

        private static int PickCellWithClosedSide(Random rng, LayoutPlan plan, List<int> candidates)
        {
            var options = candidates.FindAll(i => plan.Cells[i].OpenCount < 4 && plan.Cells[i].Feature == CellFeature.None);
            return options.Count == 0 ? -1 : options[rng.Next(options.Count)];
        }

        private static void Shuffle(Random rng, List<int> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
