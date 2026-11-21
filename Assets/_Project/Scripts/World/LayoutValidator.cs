using System.Collections.Generic;

namespace Vchod.World
{
    /// <summary>
    /// Safety net for generated floors: everything is reachable from the stairwell,
    /// the player's door and a window exist, walls and openings agree.
    /// Returns human-readable problems; empty list = valid.
    /// </summary>
    public static class LayoutValidator
    {
        public static List<string> Validate(LayoutPlan plan, LayoutRules rules)
        {
            var errors = new List<string>();
            if (plan == null || plan.Count == 0)
            {
                errors.Add("empty plan");
                return errors;
            }

            var start = plan.Start;
            if (start.Pos != new GridPos(0, 0)) errors.Add($"stairwell not at origin: {start.Pos}");
            if (start.Feature != CellFeature.Stairwell) errors.Add("cell 0 is not the stairwell");

            int playerDoors = 0, windows = 0, apartmentDoors = 0, coSources = 0, hiding = 0, hidingOnMain = 0;
            var seen = new HashSet<GridPos>();
            for (var i = 0; i < plan.Count; i++)
            {
                var c = plan.Cells[i];
                if (!seen.Add(c.Pos)) errors.Add($"duplicate cell {c.Pos}");
                if (c.Pos.ChebyshevLength > rules.gridRadius) errors.Add($"cell {c.Pos} outside radius {rules.gridRadius}");

                foreach (var d in DirectionUtil.All)
                {
                    if (!c.IsOpen(d)) continue;
                    var n = plan.IndexOf(c.Pos + d.Offset());
                    if (n < 0) errors.Add($"cell {c.Pos} opens {d} into nothing");
                    else if (!plan.Cells[n].IsOpen(d.Opposite())) errors.Add($"cell {c.Pos} opens {d} but neighbour is walled");
                }

                if (c.Feature != CellFeature.None && c.IsOpen(c.FeatureSide))
                    errors.Add($"{c.Feature} at {c.Pos} sits on an open side ({c.FeatureSide})");

                switch (c.Feature)
                {
                    case CellFeature.PlayerDoor: playerDoors++; break;
                    case CellFeature.Window: windows++; break;
                    case CellFeature.ApartmentDoor: apartmentDoors++; break;
                    case CellFeature.Stairwell when i != LayoutPlan.StartIndex: errors.Add($"extra stairwell at {c.Pos}"); break;
                }
                if (c.Hiding != HidingKind.None)
                {
                    hiding++;
                    if (c.MainPath) hidingOnMain++;
                    if (c.IsOpen(c.HidingSide)) errors.Add($"hiding spot at {c.Pos} on an open side");
                    if (c.Feature != CellFeature.None && c.FeatureSide == c.HidingSide) errors.Add($"hiding spot at {c.Pos} blocks its {c.Feature}");
                    if (i == LayoutPlan.StartIndex || c.Feature == CellFeature.PlayerDoor) errors.Add($"hiding spot in {c.Feature} cell");
                }
                if (c.IsCoSource)
                {
                    coSources++;
                    if (c.Feature != CellFeature.ApartmentDoor) errors.Add($"CO source at {c.Pos} is not an apartment door");
                }
            }

            if (playerDoors != 1) errors.Add($"expected 1 player door, found {playerDoors}");
            if (windows < 1) errors.Add("no window");
            if (apartmentDoors < rules.minApartmentDoors) errors.Add($"only {apartmentDoors} apartment doors");
            if (coSources != 1) errors.Add($"expected 1 CO source, found {coSources}");
            if (hiding < rules.minHidingSpots) errors.Add($"only {hiding} hiding spots");
            if (rules.minHidingSpots > 0 && hidingOnMain == 0) errors.Add("no hiding spot on the main path");

            var mainLength = plan.MainPath.Count - 1;
            if (mainLength < rules.mainPathMin || mainLength > rules.mainPathMax)
                errors.Add($"main path length {mainLength} outside {rules.mainPathMin}-{rules.mainPathMax}");

            var reachable = Reachable(plan);
            if (reachable.Count != plan.Count) errors.Add($"{plan.Count - reachable.Count} cells unreachable from stairwell");
            return errors;
        }

        /// <summary>Breadth-first walk through openings from the stairwell.</summary>
        public static HashSet<int> Reachable(LayoutPlan plan)
        {
            var visited = new HashSet<int> { LayoutPlan.StartIndex };
            var queue = new Queue<int>();
            queue.Enqueue(LayoutPlan.StartIndex);
            while (queue.Count > 0)
            {
                var c = plan.Cells[queue.Dequeue()];
                foreach (var d in DirectionUtil.All)
                {
                    if (!c.IsOpen(d)) continue;
                    var n = plan.IndexOf(c.Pos + d.Offset());
                    if (n >= 0 && plan.Cells[n].IsOpen(d.Opposite()) && visited.Add(n)) queue.Enqueue(n);
                }
            }
            return visited;
        }
    }
}
