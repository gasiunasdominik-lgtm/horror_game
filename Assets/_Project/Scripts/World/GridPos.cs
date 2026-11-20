using System;

namespace Vchod.World
{
    /// <summary>Integer cell coordinate on one floor. X = east, Z = north.</summary>
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int X;
        public readonly int Z;

        public GridPos(int x, int z)
        {
            X = x;
            Z = z;
        }

        public static GridPos operator +(GridPos a, GridPos b) => new(a.X + b.X, a.Z + b.Z);
        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);

        public int ChebyshevLength => Math.Max(Math.Abs(X), Math.Abs(Z));

        public bool Equals(GridPos other) => X == other.X && Z == other.Z;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => (X * 397) ^ Z;
        public override string ToString() => $"({X},{Z})";
    }

    public enum Direction
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    public static class DirectionUtil
    {
        public static readonly Direction[] All = { Direction.North, Direction.East, Direction.South, Direction.West };

        public static Direction Opposite(this Direction d) => (Direction)(((int)d + 2) % 4);
        public static Direction TurnRight(this Direction d) => (Direction)(((int)d + 1) % 4);
        public static Direction TurnLeft(this Direction d) => (Direction)(((int)d + 3) % 4);

        public static GridPos Offset(this Direction d) => d switch
        {
            Direction.North => new GridPos(0, 1),
            Direction.East => new GridPos(1, 0),
            Direction.South => new GridPos(0, -1),
            _ => new GridPos(-1, 0),
        };

        /// <summary>Unity yaw (degrees) that faces this direction.</summary>
        public static float Yaw(this Direction d) => (int)d * 90f;
    }
}
