using Stride.Core.Mathematics;

namespace TurtleGames.VoxelEngine;

public class ChunkGenerator
{
    private readonly int _seed;
    private readonly int _height;
    private readonly Vector2 _chunkSize;
    private readonly FastNoiseLite _noiseGenerator;

    public ChunkGenerator(int seed, int height,  Vector2 chunkSize)
    {
        _seed = seed;
        _height = height;
        _chunkSize = chunkSize;
        _noiseGenerator = new FastNoiseLite(_seed);
        _noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }

    public ChunkData GenerateChunk(int chunkX, int chunkZ)
    {
        ChunkData chunkData = new();
        chunkData.Size = _chunkSize;
        chunkData.Position = new ChunkVector(chunkX, chunkZ);
        chunkData.Height = _height;
        chunkData.Chunk = new int[(int)_chunkSize.X, _height, (int)_chunkSize.Y];

        float scale = 0.35f;
        var chunkPosition = new Vector2(chunkX, chunkZ) * _chunkSize;
        for (int x = 0; x < _chunkSize.X; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                for (int z = 0; z < _chunkSize.Y; z++)
                {
                    float sampleX = (x + chunkPosition.X) / scale;
                    float sampleY = y / scale;
                    float sampleZ = (z + chunkPosition.Y) / scale;//No z position always render top to bottom
                    if (_noiseGenerator.GetNoise(sampleX, sampleY, sampleZ) > 0f)
                    {
                        chunkData.Chunk[x, y, z] = 1;
                    }
                }
            }
        }

        return chunkData;
    }
}