using Stride.Core.Mathematics;

namespace TurtleGames.VoxelEngine;

public struct ChunkData
{
    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }
    public int[,,] Chunk { get; set; }
}