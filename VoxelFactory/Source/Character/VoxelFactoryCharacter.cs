using System.Linq;
using Stride.CommunityToolkit.Engine;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Physics;
using TurtleGames.VoxelEngine;

namespace VoxelFactory.Source.Character;

public class VoxelFactoryCharacter : SyncScript
{
    public ChunkSystemComponent ChunkSystemComponent { get; set; }
    public CollisionFilterGroupFlags CollideWithGroup { get; set; }
    private Simulation _simulation;

    public override void Start()
    {
        _simulation = this.GetSimulation();
    }

    public override void Update()
    {
        if (Input.HasMouse)
        {
            var camera = Entity.GetChildren().Select(b => b.Get<CameraComponent>()).First(b => b != null);

            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                var raycastResult = ScreenPositionToWorldPositionRaycast(new Vector2(0.5f,0.5f), camera, _simulation);
                    /*camera.Raycast(this,
                    camera.ScreenToWorldPoint(new Vector3(Game.Window.ClientBounds.X / 2f,
                        Game.Window.ClientBounds.Y / 2f, 0f)).XZ(),
                    collisionFilterGroupFlags: CollideWithGroup);*/

                if (raycastResult.Succeeded)
                {
                    var point = ChunkSystemComponent.PointToChunkPosition(raycastResult.Point);
                    var chunkData = point.ChunkData;
                    var chunkDataFromRay = raycastResult.Collider.Entity.Get<ChunkVisual>().ChunkData;

                 /*   for (int x = 0; x < 16; x++)
                    {
                        for (int y = 0; y < 16; y++)
                        {*/
                            chunkDataFromRay.Chunk[ (int)point.Block.X, (int)point.Block.Y,  (int)point.Block.Z] = 0;
                   /*     }
                    }*/

                    raycastResult.Collider.Entity.Get<ChunkVisual>().Remesh();
                }
            }
        }
    }
    public static HitResult ScreenPositionToWorldPositionRaycast(Vector2 screenPos, CameraComponent camera, Simulation simulation)
    {
        Matrix invViewProj = Matrix.Invert(camera.ViewProjectionMatrix);
    
        // Reconstruct the projection-space position in the (-1, +1) range.
        //    Don't forget that Y is down in screen coordinates, but up in projection space
        Vector3 sPos;
        sPos.X = screenPos.X * 2f - 1f;
        sPos.Y = 1f - screenPos.Y * 2f;
    
        // Compute the near (start) point for the raycast
        // It's assumed to have the same projection space (x,y) coordinates and z = 0 (lying on the near plane)
        // We need to unproject it to world space
        sPos.Z = 0f;
        var vectorNear = Vector3.Transform(sPos, invViewProj);
        vectorNear /= vectorNear.W;
    
        // Compute the far (end) point for the raycast
        // It's assumed to have the same projection space (x,y) coordinates and z = 1 (lying on the far plane)
        // We need to unproject it to world space
        sPos.Z = 1f;
        var vectorFar = Vector3.Transform(sPos, invViewProj);
        vectorFar /= vectorFar.W;
    
        // Raycast from the point on the near plane to the point on the far plane and get the collision result
        var result = simulation.Raycast(vectorNear.XYZ(), vectorFar.XYZ());
        return result;
    }
}