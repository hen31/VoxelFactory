using System.Collections.Generic;
using Stride.Core;
using Stride.Engine;

namespace TurtleGames.VoxelEngine;

public class VoxelGameState : StartupScript
{
    [DataMemberIgnore]
    public Dictionary<ChunkVector, ChunkData> Chunks { get; set; } = new();
}