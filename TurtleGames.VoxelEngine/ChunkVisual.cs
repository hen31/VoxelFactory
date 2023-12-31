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
        // Declared public member fields and properties will show in the game studio
        public float VoxelSize { get; set; } = 1f;

        [DataMemberIgnore] public ChunkData ChunkData { get; set; }
        public Material Material { get; set; }

        public override void Start()
        {
            GenerateVisuals();
        }


        private void GenerateVisuals()
        {
            var modelComponent = Entity.GetOrCreate<ModelComponent>();


            var vertices = new List<VertexPositionTexture>();
            var indexes = new List<int>();

            CalculateModel(vertices, indexes);
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

        private void CalculateModel(List<VertexPositionTexture> vertices, List<int> indexes)
        {
            var offSet = ChunkData.Size / 2f * VoxelSize;
            for (int x = 0; x < ChunkData.Size.X; x++)
            {
                for (int y = 0; y < ChunkData.Height; y++)
                {
                    for (int z = 0; z < ChunkData.Size.Y; z++)
                    {
                        var neighbours = new int[]
                        {
                            0, //Left
                            0, //Back
                            0, //Right
                            0, //Front
                            0, //Top
                            0 //Bottom
                        };
                        
                        neighbours[0] = x == 0 ? 1 : ChunkData.Chunk[x - 1, y, z];
                        neighbours[2] = x == (int)ChunkData.Size.X - 1 ? 1 : ChunkData.Chunk[x + 1, y, z];

                        neighbours[1] = z == 0 ? 1 : ChunkData.Chunk[x, y, z - 1];
                        neighbours[3] = z == (int)ChunkData.Size.Y - 1 ? 1 : ChunkData.Chunk[x, y, z + 1];

                        neighbours[5] = y == 0 ? 1 : ChunkData.Chunk[x, y - 1, z];
                        neighbours[4] = y == ChunkData.Height - 1 ? 1 : ChunkData.Chunk[x, y + 1, z];

                        if (ChunkData.Chunk[x, y, z] == 1)
                        {
                            CreateCubeMesh(vertices, indexes, neighbours, x, y, z, -offSet);
                        }
                    }
                }
            }
        }

        private void CreateCubeMesh(List<VertexPositionTexture> vertices, List<int> indexes, int[] neighBours, int x,
            int y, int z,
            Vector2 offSet)
        {
            var offsetWithHeight = new Vector3(offSet.X, 0, offSet.Y);
            if (neighBours[2] == 0)
            {
                AddRightSide(vertices, indexes, x, y, z, offsetWithHeight);
            }

            if (neighBours[0] == 0)
            {
                AddLeftSide(vertices, indexes, x, y, z, offsetWithHeight);
            }

            if (neighBours[4] == 0)
            {
                AddTopSide(vertices, indexes, x, y, z, offsetWithHeight);
            }

            if (neighBours[5] == 0)
            {
                AddBottomSide(vertices, indexes, x, y, z, offsetWithHeight);
            }

            if (neighBours[3] == 0)
            {
                AddFrontSide(vertices, indexes, x, y, z, offsetWithHeight);
            }

            if (neighBours[1] == 0)
            {
                AddBackSide(vertices, indexes, x, y, z, offsetWithHeight);
            }
        }

        private void AddBackSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
            Vector3 offSet)
        {
            var startIndex = vertices.Count;
            var vectorOfPosition = new Vector3((x * VoxelSize), (y * VoxelSize), (z * VoxelSize));
            var sideVertexes = new VertexPositionTexture[4];
            sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
            sideVertexes[0].TextureCoordinate = new Vector2(1, 1);

            sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, VoxelSize, 0f) + offSet;
            sideVertexes[1].TextureCoordinate = new Vector2(1, 0);

            sideVertexes[2].Position = vectorOfPosition + new Vector3(VoxelSize, VoxelSize, 0f) + offSet;
            sideVertexes[2].TextureCoordinate = new Vector2(0, 0);

            sideVertexes[3].Position = vectorOfPosition + new Vector3(VoxelSize, 0f, 0f) + offSet;
            sideVertexes[3].TextureCoordinate = new Vector2(0, 1);

            int[] indices = { 1, 2, 0, 0, 2, 3 };

            vertices.AddRange(sideVertexes);
            indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
        }

        private void AddFrontSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
            Vector3 offSet)
        {
            var startIndex = vertices.Count;
            var vectorOfPosition = new Vector3((x * VoxelSize), (y * VoxelSize), (z * VoxelSize));
            var sideVertexes = new VertexPositionTexture[4];
            sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, VoxelSize) + offSet;
            sideVertexes[0].TextureCoordinate = new Vector2(0, 1);

            sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, VoxelSize, VoxelSize) + offSet;
            sideVertexes[1].TextureCoordinate = new Vector2(0, 0);

            sideVertexes[2].Position = vectorOfPosition + new Vector3(VoxelSize, VoxelSize, VoxelSize) + offSet;
            sideVertexes[2].TextureCoordinate = new Vector2(1, 0);

            sideVertexes[3].Position = vectorOfPosition + new Vector3(VoxelSize, 0f, VoxelSize) + offSet;
            sideVertexes[3].TextureCoordinate = new Vector2(1, 1);

            int[] indices = { 1, 2, 0, 0, 2, 3 };

            vertices.AddRange(sideVertexes);
            indexes.AddRange(indices.Select(b => b + startIndex));
        }

        private void AddBottomSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
            Vector3 offSet)
        {
            var startIndex = vertices.Count;
            var vectorOfPosition = new Vector3((x * VoxelSize), (y * VoxelSize), (z * VoxelSize));
            var sideVertexes = new VertexPositionTexture[4];
            sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
            sideVertexes[0].TextureCoordinate = new Vector2(0, 1);

            sideVertexes[1].Position = vectorOfPosition + new Vector3(VoxelSize, 0f, 0f) + offSet;
            sideVertexes[1].TextureCoordinate = new Vector2(1, 1);

            sideVertexes[2].Position = vectorOfPosition + new Vector3(VoxelSize, 0f, VoxelSize) + offSet;
            sideVertexes[2].TextureCoordinate = new Vector2(1, 0);

            sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, 0f, VoxelSize) + offSet;
            sideVertexes[3].TextureCoordinate = new Vector2(0, 0);


            int[] indices = { 1, 2, 0, 0, 2, 3 };

            vertices.AddRange(sideVertexes);
            indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
        }

        private void AddTopSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
            Vector3 offSet)
        {
            var startIndex = vertices.Count;
            var vectorOfPosition = new Vector3((x * VoxelSize), (y * VoxelSize), (z * VoxelSize));
            var sideVertexes = new VertexPositionTexture[4];
            sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, VoxelSize, 0f) + offSet;
            sideVertexes[0].TextureCoordinate = new Vector2(0, 0);

            sideVertexes[1].Position = vectorOfPosition + new Vector3(VoxelSize, VoxelSize, 0f) + offSet;
            sideVertexes[1].TextureCoordinate = new Vector2(1, 0);

            sideVertexes[2].Position = vectorOfPosition + new Vector3(VoxelSize, VoxelSize, VoxelSize) + offSet;
            sideVertexes[2].TextureCoordinate = new Vector2(1, 1);

            sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, VoxelSize, VoxelSize) + offSet;
            sideVertexes[3].TextureCoordinate = new Vector2(0, 1);


            int[] indices = { 1, 2, 0, 0, 2, 3 };

            vertices.AddRange(sideVertexes);
            indexes.AddRange(indices.Select(b => b + startIndex));
        }

        private void AddRightSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
            Vector3 offSet)
        {
            var startIndex = vertices.Count;
            var vectorOfPosition = new Vector3((x * VoxelSize), (y * VoxelSize), (z * VoxelSize));
            var sideVertexes = new VertexPositionTexture[4];
            sideVertexes[0].Position = vectorOfPosition + new Vector3(VoxelSize, 0f, VoxelSize) + offSet;
            sideVertexes[0].TextureCoordinate = new Vector2(0, 1);

            sideVertexes[1].Position = vectorOfPosition + new Vector3(VoxelSize, VoxelSize, 0f) + offSet;
            sideVertexes[1].TextureCoordinate = new Vector2(1, 0);

            sideVertexes[2].Position = vectorOfPosition + new Vector3(VoxelSize, VoxelSize, VoxelSize) + offSet;
            sideVertexes[2].TextureCoordinate = new Vector2(0, 0);

            sideVertexes[3].Position = vectorOfPosition + new Vector3(VoxelSize, 0f, 0f) + offSet;
            sideVertexes[3].TextureCoordinate = new Vector2(1, 1);

            int[] indices = { 0, 2, 1, 0, 1, 3 };

            vertices.AddRange(sideVertexes);
            indexes.AddRange(indices.Select(b => b + startIndex));
        }

        private void AddLeftSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
            Vector3 offSet)
        {
            var startIndex = vertices.Count;
            var vectorOfPosition = new Vector3((x * VoxelSize), (y * VoxelSize), (z * VoxelSize));
            var sideVertexes = new VertexPositionTexture[4];
            sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, VoxelSize) + offSet;
            sideVertexes[0].TextureCoordinate = new Vector2(1, 1);

            sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, VoxelSize, 0f) + offSet;
            sideVertexes[1].TextureCoordinate = new Vector2(0, 0);

            sideVertexes[2].Position = vectorOfPosition + new Vector3(0f, VoxelSize, VoxelSize) + offSet;
            sideVertexes[2].TextureCoordinate = new Vector2(1, 0);

            sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
            sideVertexes[3].TextureCoordinate = new Vector2(0, 1);

            int[] indices = { 0, 2, 1, 0, 1, 3 };

            vertices.AddRange(sideVertexes);
            indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
        }


        public override void Update()
        {
            // Do stuff every new frame

            /*      if (Input.IsKeyPressed(Keys.L))
                  {
                      var transform = Entity.Get<TransformComponent>();
                      transform.Position = transform.Position + new Vector3(1f, 0f, 0);
                      FillChunkData();
                      GenerateVisuals();
                  }
                  if (Input.IsKeyPressed(Keys.K))
                  {
                      var transform = Entity.Get<TransformComponent>();
                      transform.Position = transform.Position + new Vector3(-1f, 0f, 0);
                      FillChunkData();
                      GenerateVisuals();
                  }*/
        }
    }
}