using System.Collections.Generic;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Extensions;
using Stride.Graphics;
using Stride.Graphics.GeometricPrimitives;
using Stride.Rendering;

namespace TurtleGames.VoxelEngine;

public class MeshTestComponent : SyncScript
{
    public Material Material { get; set; }

    public override void Start()
    {
        var normal = Vector3.UnitY;
        List<VertexPositionNormalTexture> vertices = new();
        vertices.Add(new VertexPositionNormalTexture(new Vector3(1, 0, 1), normal, new Vector2(1, 1))); //new Vector2(1, 1)
        vertices.Add(new VertexPositionNormalTexture(new Vector3(1, 0, 0), normal, new Vector2(1, 0))); //new Vector2(1, 0)
        vertices.Add(new VertexPositionNormalTexture(new Vector3(0, 0, 0), normal, new Vector2(0, 0))); //new Vector2(0, 0)

        //vertices.Add(new VertexPositionNormalTexture(new Vector3(1, 0, 0), Vector3.UnitY, new Vector2(1, 0)));

        List<int> indexes = new()
        {
            0, 1, 2
        };


        var modelComponent = Entity.GetOrCreate<ModelComponent>();
        //modelComponent.IsShadowCaster = true;
        if (vertices.Count == 0)
        {
            return;
        }

        var indices = indexes.ToArray();

        var vertexBuffer = Buffer.Vertex.New(GraphicsDevice, vertices.ToArray(), GraphicsResourceUsage.Dynamic);

        var indexBuffer = Buffer.Index.New(GraphicsDevice, indices);

        var customMesh = new Mesh
        {
            Draw = new MeshDraw
            {
                /* Vertex buffer and index buffer setup */
                PrimitiveType = PrimitiveType.TriangleList,
                DrawCount = indices.Length,
                IndexBuffer = new IndexBufferBinding(indexBuffer, true, indices.Length),
                VertexBuffers = new[]
                {
                    new VertexBufferBinding(vertexBuffer,
                        VertexPositionNormalTexture.Layout, vertexBuffer.ElementCount)
                },
            }
        };

        var model = new Model();
        // add the mesh to the model
        model.Meshes.Add(customMesh);
        /* model.Meshes.Add(new Mesh()
         {
             Draw = GeometricPrimitive.Plane.New(GraphicsDevice, normalDirection: NormalDirection.UpY).ToMeshDraw()
         });*/
        model.Materials.Add(Material);

        modelComponent.Model = model;
    }


    public override void Update()
    {
    }
}