using Stride.Core;

namespace TurtleGames.VoxelEngine;

[DataContract("Noise and spline settings")]
public class NoiseWithSpline
{
    [DataMember] public NoiseMapSettings NoiseMapSettings { get; set; } = new();
    [DataMember] public NoiseSpline NoiseSpline { get; set; } = new();

    public NoiseWithSpline()
    {
    }

    private NoiseMap _noiseMap;

    public void Initialize()
    {
        _noiseMap = new NoiseMap(NoiseMapSettings.Seed, NoiseMapSettings.Scale)
        {
            Octaves = NoiseMapSettings.Octaves,
            Lacunarity = NoiseMapSettings.Lacunarity,
            Persistance = NoiseMapSettings.Persistance
        };
    }

    public int GetValue(float xPosition, float yPosition)
    {
        return NoiseSpline.GetValue(_noiseMap.GetNoise(xPosition, yPosition));
    }
}