using Stride.Core;

namespace TurtleGames.VoxelEngine;

[DataContract("Noise settings")]
public class NoiseMapSettings
{
    [DataMember] public int Seed { get; set; }
    [DataMember] public int Octaves { get; set; } = 4;
    [DataMember] public float Lacunarity { get; set; } = 0.5f;
    [DataMember] public float Persistance { get; set; } = 1.87f;
    [DataMember] public float Scale { get; set; } = 0.0025f;
}