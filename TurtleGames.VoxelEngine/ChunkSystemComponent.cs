using System;
using System.Diagnostics;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Rendering;

namespace TurtleGames.VoxelEngine;

public class ChunkSystemComponent : SyncScript
{
    private TransformComponent _cameraTransform;
    private ChunkGeneratorComponent _chunkGenerator;
    public CameraComponent Camera { get; set; }
    public VoxelGameState GameState { get; set; }
    private Vector2 _chunkSize;

    public int Radius { get; set; } = 2;
    public float VoxelSize { get; set; } = 1;

    public bool OnlyInitialGeneration { get; set; } = false;

    public Material BlockMaterial { get; set; }

    public override void Start()
    {
        _cameraTransform = Camera.Entity.Get<TransformComponent>();
        _chunkGenerator = Entity.Get<ChunkGeneratorComponent>();
        _chunkSize = _chunkGenerator.ChunkSize;
    }

    private bool _once;

    public override void Update()
    {
        if (OnlyInitialGeneration)
        {
            if (_once)
            {
                return;
            }
            else
            {
                _once = true;
            }
        }

        var currentPositionInChunkPositions = ToChunkPosition(_cameraTransform.Position);

        for (int x = (int)currentPositionInChunkPositions.X - Radius;
             x < currentPositionInChunkPositions.X + Radius;
             x++)
        {
            for (int y = (int)currentPositionInChunkPositions.Y - Radius;
                 y < currentPositionInChunkPositions.Y + Radius;
                 y++)
            {
                var newPosition = new ChunkVector(x, y);
                if (GameState.Chunks.All(b => b.Position != newPosition))
                {
                    var chunkData = _chunkGenerator.QueueNewChunkForCalculation(new ChunkVector(x, y));
                    GameState.Chunks.Add(chunkData);
                    var visualizationEntity = new Entity("chunkVisual",
                        new Vector3(x * _chunkSize.X * VoxelSize, -_chunkGenerator.ChunkHeight / 2f * VoxelSize,
                            y * _chunkSize.Y * VoxelSize));
                    visualizationEntity.Add(new ChunkVisual()
                    {
                        ChunkData = chunkData,
                        Material = BlockMaterial,
                        VoxelSize = VoxelSize
                    });
                    Entity.Scene.Entities.Add(visualizationEntity);
                    Debug.WriteLine(
                        $"Currentchunk check start chunk X:{currentPositionInChunkPositions.X} Y:{currentPositionInChunkPositions.Y}");

                    Debug.WriteLine($"generated chunk X:{x} Y:{y}");
                    /*     DebugText.Print($"generated chunk X:{x} Y:{y}", new Int2(x: 50, y: 50),
                             timeOnScreen: new TimeSpan(0, 0, 2));*/
                }
            }
        }
    }

    private Vector2 ToChunkPosition(Vector3 cameraTransformPosition)
    {
        int x = (int)MathF.Round(cameraTransformPosition.X / _chunkSize.X / VoxelSize);
        int y = (int)MathF.Round(cameraTransformPosition.Z / _chunkSize.Y / VoxelSize);
        return new Vector2(x, y);
    }
}