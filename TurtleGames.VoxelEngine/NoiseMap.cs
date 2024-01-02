using System;
using System.Collections.Generic;

namespace TurtleGames.VoxelEngine;

public class NoiseMap
{
    private readonly int _seed;
    private readonly float _scale;
    private readonly FastNoiseLite _noiseGenerator;

    public NoiseMap(int seed, float scale)
    {
        _seed = seed;
        _scale = scale;
        _noiseGenerator = new FastNoiseLite(_seed);
        _noiseGenerator.SetFrequency(_scale);
    }


    public float GetNoise(float xPosition, float yPosition)
    {
        return _noiseGenerator.GetNoise(xPosition, yPosition);
    }
    
    
}