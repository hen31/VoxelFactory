using Stride.Core.Mathematics;

namespace TurtleGames.VoxelEngine;

public static class Extensions
{
    public static bool EqualsWithMargin(this Vector2 vector, Vector2 other)
    {
        return (vector - other).Length() < 0.001f;
    }
    public static ChunkVector ToChunkVector(this Vector2 vector)
    {
        return new ChunkVector((int)vector.X, (int)vector.Y);
    }
    
}