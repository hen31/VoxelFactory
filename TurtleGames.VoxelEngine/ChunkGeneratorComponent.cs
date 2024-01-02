using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BulletSharp;
using Stride.Animations;
using Stride.Core;
using Stride.Core.Annotations;
using Stride.Core.Mathematics;
using Stride.Core.Threading;
using Stride.Engine;
using Stride.Graphics;
using Stride.Input;
using Stride.Particles;
using Stride.Particles.Components;
using Color = Stride.Core.Mathematics.Color;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace TurtleGames.VoxelEngine;

public class ChunkGeneratorComponent : SyncScript
{
    public int Seed { get; set; }
    public ushort ChunkHeight { get; set; }
    public Vector2 ChunkSize { get; set; }
    public bool DebugWrite { get; set; }

    [DataMember("Continental spline")] public SplineSettings ContinentalSplineSettings { get; set; } = new();

    private ConcurrentQueue<ChunkData> _calculationQueue = new ConcurrentQueue<ChunkData>();
    private Thread _thread;
    private CancellationTokenSource _cancellationToken;
    private NoiseMap _continentalness;

    private void InitializeNoiseGenerators()
    {
        _continentalness = new NoiseMap(Seed, 0.0025f);
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
                int baseHeight = 100 + ContinentalSplineSettings.GetValue(continentalness);
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
                    var value = (int)MathUtils.Map(ContinentalSplineSettings.GetValue(_continentalness.GetNoise(x, y)), ContinentalSplineSettings.SplinePoints.Min(b=> b.Value), ContinentalSplineSettings.SplinePoints.Max(b=> b.Value), 0, 254);
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

[DataContract("Spline")]
public class SplineSettings
{
    [DataMember]
    [DataMemberRange(-1f, 1f, 0.05f, 0.1f, 2)]
    public float MinValue { get; set; } = -1f;

    [DataMember]
    [DataMemberRange(-1f, 1f, 0.05f, 0.1f, 2)]
    public float MaxValue { get; set; } = -1f;

    [DataMember] public List<SplinePoint> SplinePoints = new();

    public ushort GetValue(float splinePoint)
    {
        SplinePoint fromPoint = null;
        SplinePoint toPoint = null;
        var orderPoints = SplinePoints.OrderBy(b => b.Point).ToArray();
        for (int i = 0; i < orderPoints.Length; i++)
        {
            var point = orderPoints[i];
            if (point.Point > splinePoint)
            {
                if (i != 0)
                {
                    fromPoint = orderPoints[i - 1];
                }

                toPoint = orderPoints[i];

                break;
            }
        }

        if (fromPoint == null)
        {
            fromPoint = toPoint;
        }

        var ifZeroThanThisWasNextPoint = (toPoint.Point - fromPoint.Point);
        var ifZeroThanThisIsPoint = (splinePoint - fromPoint.Point);
        var positionBetweenPoints = ifZeroThanThisIsPoint / ifZeroThanThisWasNextPoint;

        var ifZeroThanThisWasNextValue = (toPoint.Value - fromPoint.Value);
        return (ushort)(fromPoint.Value + (ifZeroThanThisWasNextValue * positionBetweenPoints));
    }
}

[DataContract("Spline point")]
public class SplinePoint
{
    [DataMember]
    [DataMemberRange(-1f, 1f, 0.05f, 0.1f, 2)]
    public float Point { get; set; }

    [DataMember] public ushort Value { get; set; }
}