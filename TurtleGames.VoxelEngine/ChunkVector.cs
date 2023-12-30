using System;

namespace TurtleGames.VoxelEngine;

public struct ChunkVector
{
    public ChunkVector()
    {
    }

    public ChunkVector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }

    public override bool Equals(object obj)
    {
        return obj is ChunkVector cVector && cVector.X == X && cVector.Y == Y;
    }

    public bool Equals(ChunkVector other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public static bool operator ==(ChunkVector left, ChunkVector right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ChunkVector left, ChunkVector right)
    {
        return !(left == right);
    }
}