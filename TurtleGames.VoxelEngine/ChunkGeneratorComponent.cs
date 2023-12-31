using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using BulletSharp;
using Stride.Core.Mathematics;
using Stride.Core.Threading;
using Stride.Engine;

namespace TurtleGames.VoxelEngine;

public class ChunkGeneratorComponent : StartupScript
{
    public int Seed { get; set; }
    public int ChunkHeight { get; set; }
    public Vector2 ChunkSize { get; set; }
    private FastNoiseLite _noiseGenerator;


    private ConcurrentQueue<ChunkData> _calculationQueue = new ConcurrentQueue<ChunkData>();
    private Thread _thread;
    private CancellationTokenSource _cancellationToken;

    public ChunkData QueueNewChunkForCalculation(ChunkVector position)
    {
        var chunk = new ChunkData()
        {
            Size = ChunkSize,
            Position = position,
            Height = ChunkHeight,
            Chunk = new int[(int)ChunkSize.X, ChunkHeight, (int)ChunkSize.Y],
            Calculated = false
        };
        _calculationQueue.Enqueue(chunk);
        return chunk;
    }


    public void GenerateChunk(ChunkData chunkData)
    {
        float scale = 0.35f;
        var chunkPosition = new Vector2(chunkData.Position.X, chunkData.Position.Y) * ChunkSize;
        for (int x = 0; x < ChunkSize.X; x++)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                for (int z = 0; z < ChunkSize.Y; z++)
                {
                    float sampleX = (x + chunkPosition.X) / scale;
                    float sampleY = y / scale;
                    float sampleZ = (z + chunkPosition.Y) / scale; //No z position always render top to bottom
                    float noiseValue = _noiseGenerator.GetNoise(sampleX, sampleY, sampleZ);

                    if (y > 500)
                    {
                        chunkData.Chunk[x, y, z] = 0;
                    }
                    else if (y > 256 && noiseValue > 0.8f)
                    {
                        chunkData.Chunk[x, y, z] = 1;
                    }
                    else if (noiseValue > 0f)
                    {
                        chunkData.Chunk[x, y, z] = 1;
                    }
                }
            }
        }
    }

    public override void Start()
    {
        InitializeNoiseGenerator();
        _cancellationToken = new CancellationTokenSource();
        var token = _cancellationToken.Token;
        Task.Factory.StartNew(() => RunCalculationThread(token), TaskCreationOptions.LongRunning);
    }

    private void RunCalculationThread(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            RunCalculations();
        }
    }

    public override void Cancel()
    {
        _cancellationToken.Cancel();
        _cancellationToken.Dispose();
    }

    public void RunCalculations()
    {
        if (_calculationQueue.TryDequeue(out ChunkData toCalculate))
        {
            GenerateChunk(toCalculate);
            toCalculate.Calculated = true;
        }
    }

    private void InitializeNoiseGenerator()
    {
        _noiseGenerator = new FastNoiseLite(Seed);
        _noiseGenerator.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
    }
}