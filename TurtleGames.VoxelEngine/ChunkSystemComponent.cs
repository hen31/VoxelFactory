using System;
using System.Diagnostics;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;

namespace TurtleGames.VoxelEngine;

public class ChunkSystemComponent : SyncScript
{
    private TransformComponent _cameraTransform;
    private ChunkGenerator _chunkGenerator;
    public CameraComponent Camera { get; set; }
    public VoxelGameState GameState { get; set; }
    public Vector2 ChunkSize { get; set; }
    public int Seed { get; set; }
    public int ChunkHeight { get; set; }
    public int Radius { get; set; } = 2;


    public override void Start()
    {
        _cameraTransform = Camera.Entity.Get<TransformComponent>();
        _chunkGenerator = new ChunkGenerator(Seed, ChunkHeight, ChunkSize);
    }

    public override void Update()
    {
     
        var currentPositionInChunkPositions = ToChunkPosition(_cameraTransform.Position);

        for (int x = (int)currentPositionInChunkPositions.X - Radius; x < currentPositionInChunkPositions.X + Radius; x++)
        {
            for (int y = (int)currentPositionInChunkPositions.Y - Radius; y < currentPositionInChunkPositions.Y + Radius; y++)
            {
                var newPosition = new ChunkVector(x, y);
                if (GameState.Chunks.All(b => b.Position != newPosition))
                {
                    var chunk = _chunkGenerator.GenerateChunk(x, y);
                    GameState.Chunks.Add(chunk);
                    var visualizationEntity = new Entity("chunkVisual",
                        new Vector3(x * ChunkSize.X, -ChunkHeight / 2f,
                            y * ChunkSize.Y));
                    visualizationEntity.Add(new ChunkVisual()
                    {
                        ChunkData = chunk
                    });
                    Entity.Scene.Entities.Add(visualizationEntity);
                    Debug.WriteLine($"Currentchunk check start chunk X:{currentPositionInChunkPositions.X} Y:{currentPositionInChunkPositions.Y}");

                    Debug.WriteLine($"generated chunk X:{x} Y:{y}");
               /*     DebugText.Print($"generated chunk X:{x} Y:{y}", new Int2(x: 50, y: 50),
                        timeOnScreen: new TimeSpan(0, 0, 2));*/
                }
            }
        }
      //  Debug.WriteLine($"Currentchunk check END");


        //TODO: ...
    }

    private Vector2 ToChunkPosition(Vector3 cameraTransformPosition)
    {
        int x = (int)MathF.Round(cameraTransformPosition.X / ChunkSize.X);
        int y = (int)MathF.Round(cameraTransformPosition.Z / ChunkSize.Y);
        return new Vector2(x, y);
    }
}