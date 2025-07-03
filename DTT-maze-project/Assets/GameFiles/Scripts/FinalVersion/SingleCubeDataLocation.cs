using System;
using UnityEngine;

/// <summary>
/// Represents a grid coordinate (X, Z) using integer (or float) values.
/// We override Equals and GetHashCode so that two instances with the same
/// X and Z are treated as “the same” when you put them in a HashSet or Dictionary.
/// </summary>

public struct SingleCubeDataLocation : IEquatable<SingleCubeDataLocation>
{
    public int X { get; }
    public int Z { get; }

    public SingleCubeDataLocation(int x, int z)
    {
        X = x;
        Z = z;
    }

    public bool Equals(SingleCubeDataLocation other)
        => X == other.X && Z == other.Z;

    public override bool Equals(object obj)
        => obj is SingleCubeDataLocation o && Equals(o);

    public override int GetHashCode()
        => HashCode.Combine(X, Z);

    public override string ToString() => $"({X},{Z})";

    public static bool operator ==(SingleCubeDataLocation a, SingleCubeDataLocation b) => a.Equals(b);
    public static bool operator !=(SingleCubeDataLocation a, SingleCubeDataLocation b) => !a.Equals(b);
}