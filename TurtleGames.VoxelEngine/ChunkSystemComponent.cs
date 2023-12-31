using System;
using System.Collections.Generic;
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
    private ChunkVisualsGeneratorComponent _chunkVisualGenerator;
    public CameraComponent Camera { get; set; }
    public VoxelGameState GameState { get; set; }
    private Vector2 _chunkSize;

    public int Radius { get; set; } = 2;
    public float VoxelSize { get; set; } = 1;

    public bool OnlyInitialGeneration { get; set; } = false;

    public Material BlockMaterial { get; set; }
    
    private List<ChunkVisual> _currentVisuals  = new List<ChunkVisual>();

    public override void Start()
    {
        _cameraTransform = Camera.Entity.Get<TransformComponent>();
        _chunkGenerator = Entity.Get<ChunkGeneratorComponent>();
        _chunkVisualGenerator = Entity.Get<ChunkVisualsGeneratorComponent>();
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
        var toDelete = _currentVisuals.ToList();
        
        for (int x = (int)currentPositionInChunkPositions.X - Radius;
             x < currentPositionInChunkPositions.X + Radius;
             x++)
        {
            for (int y = (int)currentPositionInChunkPositions.Y - Radius;
                 y < currentPositionInChunkPositions.Y + Radius;
                 y++)
            {
                var newPosition = new ChunkVector(x, y);
                ChunkData chunkData = null;
                
                if (!GameState.Chunks.TryGetValue(newPosition, out chunkData))
                {
                    chunkData = _chunkGenerator.QueueNewChunkForCalculation(new ChunkVector(x, y));
                    GameState.Chunks.Add(newPosition, chunkData);
                }

                var currentVisual = _currentVisuals.FirstOrDefault(b => b.ChunkData == chunkData);
                if (currentVisual !=null)
                {
                    toDelete.Remove(currentVisual);
                }
                else
                {
                    var visualizationEntity = new Entity("chunkVisual",
                        new Vector3(x * _chunkSize.X * VoxelSize, -_chunkGenerator.ChunkHeight / 2f * VoxelSize,
                            y * _chunkSize.Y * VoxelSize));
                    var chunkVisualization = new ChunkVisual()
                    {
                        ChunkData = chunkData,
                        Material = BlockMaterial,
                        VoxelSize = VoxelSize,
                        GeneratorComponent = _chunkVisualGenerator
                    };
                    visualizationEntity.Add(chunkVisualization);
                    Entity.Scene.Entities.Add(visualizationEntity);
                    _currentVisuals.Add(chunkVisualization);
                }
            }
        }


        foreach (var visualNotLongerInRange in toDelete)
        {
            Entity.Scene.Entities.Remove(visualNotLongerInRange.Entity);
            _currentVisuals.Remove(visualNotLongerInRange);
        }
    }   

    private Vector2 ToChunkPosition(Vector3 cameraTransformPosition)
    {
        int x = (int)MathF.Round(cameraTransformPosition.X / _chunkSize.X / VoxelSize);
        int y = (int)MathF.Round(cameraTransformPosition.Z / _chunkSize.Y / VoxelSize);
        return new Vector2(x, y);
    }
}