using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using BulletSharp;
using Stride.Core.Mathematics;
using Stride.Core.Threading;
using Stride.Engine;
using Stride.Graphics;
using Stride.Input;
using Color = Stride.Core.Mathematics.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace TurtleGames.VoxelEngine;

public class ChunkGeneratorComponent : SyncScript
{
    public int Seed { get; set; }
    public ushort ChunkHeight { get; set; }
    public Vector2 ChunkSize { get; set; }


    public bool DebugWrite { get; set; }

    private ConcurrentQueue<ChunkData> _calculationQueue = new ConcurrentQueue<ChunkData>();
    private Thread _thread;
    private CancellationTokenSource _cancellationToken;
    private NoiseMap _continentalness;
    private void InitializeNoiseGenerators()
    {
        _continentalness = new NoiseMap(Seed, 0.005f);
    }

    public ChunkData QueueNewChunkForCalculation(ChunkVector position)
    {
        var chunk = new ChunkData()
        {
            Size = ChunkSize,
            Position = position,
            Height = ChunkHeight,
            Chunk = new ushort[(int)ChunkSize.X, ChunkHeight, (int)ChunkSize.Y],
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
            float xPosition = (x + chunkPosition.X);
            for (int z = 0; z < ChunkSize.Y; z++)
            {
                float zPosition = (z + chunkPosition.Y);
                float continentalness = _continentalness.GetNoise(xPosition, zPosition);
                int baseHeight = 100 + (int)(continentalness * 20);
                for (int y = 0; y < ChunkHeight; y++)
                {
                    if (y < baseHeight)
                    {
                        chunkData.Chunk[x, y, z] = 1;
                    }
                    else
                    {
                        chunkData.Chunk[x, y, z] = 0;
                    }
                }
            }
        }
    }

    public override void Start()
    {
        InitializeNoiseGenerators();
        _cancellationToken = new CancellationTokenSource();
        var token = _cancellationToken.Token;
        Task.Factory.StartNew(() => RunCalculationThread(token), TaskCreationOptions.LongRunning);
    }

    public override void Update()
    {
        if (Input.IsKeyPressed(Keys.N))
        {
            var bitmap = new Bitmap(1920, 1080);

            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var value = (int)MathUtils.Map(_continentalness.GetNoise(x, y), -1, 1, 0, 254);
                    //Lerp(0,255,_continentalness.GetNoise(x,y)
                    var color = System.Drawing.Color.FromArgb(
                        value,
                        value, value);
                    bitmap.SetPixel(x, y, color);
                }
            }

            bitmap.Save("C:\\Development\\Noises\\c.bmp");
        }
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
            Debug.WriteLineIf(DebugWrite,
                $"Start calculation for cunk X:{toCalculate.Position.X},Y:{toCalculate.Position.Y}");
            GenerateChunk(toCalculate);
            toCalculate.Calculated = true;
            Debug.WriteLineIf(DebugWrite,
                $"End calculation for cunk X:{toCalculate.Position.X},Y:{toCalculate.Position.Y}");
        }
    }
}