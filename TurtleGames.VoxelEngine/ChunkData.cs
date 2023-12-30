using Stride.Core.Mathematics;

namespace TurtleGames.VoxelEngine;

public struct ChunkData
{
    public ChunkVector Position { get; set; }
    public Vector2 Size { get; set; }
    public int[,,] Chunk { get; set; }
    public int Height { get; set; }
}