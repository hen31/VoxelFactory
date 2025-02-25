﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Materials;

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

    private List<ChunkVisual> _currentVisuals = new List<ChunkVisual>();

    [DataMemberIgnore] public List<BlockType> BlockTypes = new List<BlockType>()
    {
        new BlockType()
        {
            BlockId = 1,
            Name = "Stone",
            BlockTexture = new BlockTextureInfo()
            {
                DefaultTexture = "stone.png"
            }
        },
        new BlockType()
        {
            BlockId = 2,
            Name = "Dirt",
            BlockTexture = new BlockTextureInfo()
            {
                DefaultTexture = "dirt.png"
            }
        },
        new BlockType()
        {
            BlockId = 3,
            Name = "Grass",
            BlockTexture = new BlockTextureInfo()
            {
                DefaultTexture = "dirt.png",
                TopTexture = "grass_block_top.png"
            }
        }
    };

    public override void Start()
    {
        var material =
            new BlockTextureCreator(BlockTypes).CreateBlockTextureAtlas(
                out Dictionary<ushort, BlockTextureUvMapping> blockMapping);
        var texture = Texture.New(GraphicsDevice, material);
        BlockMaterial.Passes[0].Parameters.Set(MaterialKeys.DiffuseMap, texture);
        _cameraTransform = Camera.Entity.Get<TransformComponent>();
        _chunkGenerator = Entity.Get<ChunkGeneratorComponent>();
        _chunkVisualGenerator = Entity.Get<ChunkVisualsGeneratorComponent>();
        _chunkVisualGenerator.BlockUvMapping = blockMapping;
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

        var currentPositionInChunkPositions = ToChunkPosition(_cameraTransform.LocalToWorld(Vector3.Zero));
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
                var chunkData = GetChunkAt(newPosition);


                var currentVisual = _currentVisuals.FirstOrDefault(b => b.ChunkData == chunkData);
                if (currentVisual != null)
                {
                    toDelete.Remove(currentVisual);
                }
                else
                {
                    var neighbours = new ChunkData[4];
                    neighbours[0] = GetChunkAt(new ChunkVector(x, y + 1));
                    neighbours[1] = GetChunkAt(new ChunkVector(x + 1, y));
                    neighbours[2] = GetChunkAt(new ChunkVector(x, y - 1));
                    neighbours[3] = GetChunkAt(new ChunkVector(x - 1, y));

                    var visualizationEntity = new Entity("chunkVisual",
                        new Vector3(x * _chunkSize.X * VoxelSize, -_chunkGenerator.ChunkHeight / 2f * VoxelSize,
                            y * _chunkSize.Y * VoxelSize));
                    var chunkVisualization = new ChunkVisual()
                    {
                        ChunkData = chunkData,
                        Material = BlockMaterial,
                        VoxelSize = VoxelSize,
                        GeneratorComponent = _chunkVisualGenerator,
                        Neighbours = neighbours
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

    private ChunkData GetChunkAt(ChunkVector newPosition)
    {
        ChunkData chunkData = null;

        if (!GameState.Chunks.TryGetValue(newPosition, out chunkData))
        {
            chunkData = _chunkGenerator.QueueNewChunkForCalculation(newPosition);
            GameState.Chunks.Add(newPosition, chunkData);
        }

        return chunkData;
    }

    private Vector2 ToChunkPosition(Vector3 cameraTransformPosition)
    {
        int x = (int)MathF.Round(cameraTransformPosition.X / _chunkSize.X / VoxelSize);
        int y = (int)MathF.Round(cameraTransformPosition.Z / _chunkSize.Y / VoxelSize);
        return new Vector2(x, y);
    }

    public (ChunkVector Chunk, Vector3 Block, ChunkData ChunkData) PointToChunkPosition(Vector3 point)
    {
        var chunkPosition = ToChunkPosition(point).ToChunkVector();
        var inChunkPosition = PointToInChunkPosition(point, chunkPosition);
        var chunkData = GetChunkAt(chunkPosition);

        return (chunkPosition, inChunkPosition, chunkData);
    }

    private Vector3 PointToInChunkPosition(Vector3 point, ChunkVector chunkPosition)
    {
        var pointInSideChunk = point - (chunkPosition * _chunkSize * VoxelSize).ToVector3() +
                               new Vector3(_chunkGenerator.ChunkSize.X / 2f, _chunkGenerator.ChunkHeight / 2f,
                                   _chunkGenerator.ChunkSize.Y / 2f);

        int x = (int)(pointInSideChunk.X / VoxelSize);
        int y = (int)(pointInSideChunk.Y / VoxelSize);
        int z = (int)(pointInSideChunk.Z / VoxelSize);
        return new Vector3(x, y, z);
    }

    public void DestroyBlock(ChunkData chunkData, (ChunkVector Chunk, Vector3 Block, ChunkData ChunkData) point)
    {
        chunkData.Chunk[(int)point.Block.X, (int)point.Block.Y, (int)point.Block.Z] = 0;
        _currentVisuals.FirstOrDefault(b =>
            b.ChunkData.Position.X == chunkData.Position.X && b.ChunkData.Position.Y == chunkData.Position.Y)?.Remesh();
        if ((int)point.Block.X == 0)
        {
            _currentVisuals.FirstOrDefault(b =>
                    b.ChunkData.Position.X == chunkData.Position.X - 1 &&
                    b.ChunkData.Position.Y == chunkData.Position.Y)
                ?.Remesh();
        }
        else if ((int)point.Block.X == (int)_chunkSize.X - 1)
        {
            _currentVisuals.FirstOrDefault(b =>
                    b.ChunkData.Position.X == chunkData.Position.X + 1 &&
                    b.ChunkData.Position.Y == chunkData.Position.Y)
                ?.Remesh();
        }

        if ((int)point.Block.Z == 0)
        {
            _currentVisuals.FirstOrDefault(b =>
                    b.ChunkData.Position.X == chunkData.Position.X &&
                    b.ChunkData.Position.Y == chunkData.Position.Y - 1)
                ?.Remesh();
        }
        else if ((int)point.Block.Z == (int)_chunkSize.Y - 1)
        {
            _currentVisuals.FirstOrDefault(b =>
                    b.ChunkData.Position.X == chunkData.Position.X &&
                    b.ChunkData.Position.Y == chunkData.Position.Y + 1)
                ?.Remesh();
        }
    }
}