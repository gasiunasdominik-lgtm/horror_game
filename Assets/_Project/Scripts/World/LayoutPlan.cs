using System;
using System.Collections.Generic;

namespace Vchod.World
{
    public enum CellFeature
    {
        None = 0,
        Stairwell = 1,
        ApartmentDoor = 2,
        Window = 3,
        PlayerDoor = 4,
    }

    /// <summary>Furniture the player can hide in (GDD: wardrobes and tables).</summary>
    public enum HidingKind
    {
        None = 0,
        Wardrobe = 1,
        Table = 2,
    }

    /// <summary>One corridor module slot. Pure data so layouts can be tested without a scene.</summary>
    public sealed class LayoutCell
    {
        public GridPos Pos;
        public int Parent = -1;
        /// <summary>Direction of travel from the parent into this cell (the cell's "forward").</summary>
        public Direction InDirection;
        /// <summary>Steps from the stairwell along the corridor tree.</summary>
        public int Depth;
        public bool MainPath;
        public byte Openings;
        public CellFeature Feature;
        public Direction FeatureSide;
        public bool IsCoSource;
        public HidingKind Hiding;
        public Direction HidingSide;
        public bool HasSwitch;
        public Direction SwitchSide;

        public bool IsOpen(Direction d) => (Openings & (1 << (int)d)) != 0;
        public void Open(Direction d) => Openings |= (byte)(1 << (int)d);

        public int OpenCount
        {
            get
            {
                var n = 0;
                foreach (var d in DirectionUtil.All) if (IsOpen(d)) n++;
                return n;
            }
        }
    }

    /// <summary>A generated floor: a tree of corridor cells rooted at the stairwell (index 0).</summary>
    public sealed class LayoutPlan
    {
        public const int StartIndex = 0;

        private readonly Dictionary<GridPos, int> _index = new();

        public int Seed { get; internal set; }
        /// <summary>Only the player's floor has their door and the CO source (the neighbour's flat).</summary>
        public bool IsPlayerFloor { get; internal set; } = true;
        public List<LayoutCell> Cells { get; } = new();
        /// <summary>Cell indices from the stairwell to the player's door.</summary>
        public List<int> MainPath { get; } = new();

        public LayoutCell Start => Cells[StartIndex];
        public int Count => Cells.Count;

        public int IndexOf(GridPos pos) => _index.TryGetValue(pos, out var i) ? i : -1;
        public bool IsOccupied(GridPos pos) => _index.ContainsKey(pos);

        internal int Add(LayoutCell cell)
        {
            if (_index.ContainsKey(cell.Pos)) throw new InvalidOperationException($"Cell {cell.Pos} already occupied");
            Cells.Add(cell);
            _index[cell.Pos] = Cells.Count - 1;
            return Cells.Count - 1;
        }

        public int FindFeature(CellFeature feature)
        {
            for (var i = 0; i < Cells.Count; i++)
                if (Cells[i].Feature == feature) return i;
            return -1;
        }

        public int CoSourceIndex
        {
            get
            {
                for (var i = 0; i < Cells.Count; i++)
                    if (Cells[i].IsCoSource) return i;
                return -1;
            }
        }

        /// <summary>Order-dependent FNV-1a over everything that affects geometry. Shown in the overlay and bug reports.</summary>
        public uint ComputeHash()
        {
            unchecked
            {
                var h = 2166136261u;
                foreach (var c in Cells)
                {
                    h = Mix(h, c.Pos.X);
                    h = Mix(h, c.Pos.Z);
                    h = Mix(h, c.Openings);
                    h = Mix(h, (int)c.Feature);
                    h = Mix(h, (int)c.FeatureSide);
                    h = Mix(h, c.IsCoSource ? 1 : 0);
                    h = Mix(h, (int)c.Hiding);
                    h = Mix(h, (int)c.HidingSide);
                    h = Mix(h, c.HasSwitch ? 1 + (int)c.SwitchSide : 0);
                }
                return h;
            }
        }

        private static uint Mix(uint h, int value)
        {
            unchecked
            {
                for (var i = 0; i < 4; i++)
                {
                    h ^= (uint)(value >> (i * 8)) & 0xFF;
                    h *= 16777619u;
                }
                return h;
            }
        }
    }
}
