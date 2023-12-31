using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;

namespace TurtleGames.VoxelEngine
{
    public class ChunkVisual : SyncScript
    {
        private ChunkVisualsRequest _request;

        // Declared public member fields and properties will show in the game studio
        private bool _hasModel { get; set; }
        public float VoxelSize { get; set; } = 1f;
        [DataMemberIgnore] public ChunkData ChunkData { get; set; }
        [DataMemberIgnore] public ChunkVisualsGeneratorComponent GeneratorComponent { get; set; }
        public Material Material { get; set; }
        public ChunkData[] Neighbours { get; set; }

        public override void Start()
        {
        }

        private void GenerateVisuals(List<VertexPositionTexture> vertices, List<int> indexes)
        {
            var modelComponent = Entity.GetOrCreate<ModelComponent>();

            if (vertices.Count == 0)
            {
                return;
            }

            var indices = indexes.ToArray();
            var vertexBuffer = Stride.Graphics.Buffer.Vertex.New(GraphicsDevice, vertices.ToArray(),
                GraphicsResourceUsage.Default);

            var indexBuffer = Stride.Graphics.Buffer.Index.New(GraphicsDevice, indexes.ToArray());

            var vector3OfSize = new Vector3(ChunkData.Size.X, ChunkData.Height, ChunkData.Size.Y);
            var points = new[]
            {
                vector3OfSize,
                vector3OfSize,
            };
            var boundingBox = BoundingBox.FromPoints(points);

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
                            VertexPositionTexture.Layout, vertexBuffer.ElementCount)
                    },
                },
                BoundingBox = boundingBox
            };

            var model = new Model();
            // add the mesh to the model
            model.Meshes.Add(customMesh);
            model.BoundingBox = boundingBox;
            modelComponent.Model = model;

            model.Materials.Add(Material);
        }


        public override void Update()
        {
            if (_hasModel
                || !ChunkData.Calculated
                || Neighbours.Any(b => !b.Calculated)
                || _request is { IsCalculated: false })
            {
                return;
            }

            // Do stuff every new frame
            if (_request == null)
            {
                _request = GeneratorComponent.EnequeVisualCreation(ChunkData, Neighbours);
            }
            else
            {
                _hasModel = true;
                GenerateVisuals(_request.VisualsData.Vertexes, _request.VisualsData.Indexes);
                _request = null;
            }
        }
    }
}