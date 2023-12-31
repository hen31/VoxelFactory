using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;

namespace TurtleGames.VoxelEngine;

public class ChunkVisualsGeneratorComponent : StartupScript
{
    private float _voxelSize;
    private CancellationTokenSource _cancellationToken;
    private ConcurrentQueue<ChunkVisualsRequest> _calculationQueue = new ConcurrentQueue<ChunkVisualsRequest>();

    public override void Start()
    {
        _voxelSize = Entity.Get<ChunkSystemComponent>().VoxelSize;

        _cancellationToken = new CancellationTokenSource();
        var token = _cancellationToken.Token;
        Task.Factory.StartNew(() => RunCalculationThread(token), TaskCreationOptions.LongRunning);
    }

    public override void Cancel()
    {
        _cancellationToken.Cancel();
    }

    private void RunCalculationThread(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            DoCalculation();
        }
    }

    private void DoCalculation()
    {
        if (_calculationQueue.TryDequeue(out ChunkVisualsRequest request))
        {
            ChunkVisualData chunkVisualData = new ChunkVisualData();
            chunkVisualData.Vertexes = new List<VertexPositionTexture>();
            chunkVisualData.Indexes = new List<int>();

            CalculateModel(request.ChunkData, chunkVisualData.Vertexes, chunkVisualData.Indexes);
            request.VisualsData = chunkVisualData;
            request.IsCalculated = true;
        }
    }


    private void CalculateModel(ChunkData chunkData, List<VertexPositionTexture> vertices, List<int> indexes)
    {
        var offSet = chunkData.Size / 2f * _voxelSize;
        for (int x = 0; x < chunkData.Size.X; x++)
        {
            for (int y = 0; y < chunkData.Height; y++)
            {
                for (int z = 0; z < chunkData.Size.Y; z++)
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

                    neighbours[0] = x == 0 ? 1 : chunkData.Chunk[x - 1, y, z];
                    neighbours[2] = x == (int)chunkData.Size.X - 1 ? 1 : chunkData.Chunk[x + 1, y, z];

                    neighbours[1] = z == 0 ? 1 : chunkData.Chunk[x, y, z - 1];
                    neighbours[3] = z == (int)chunkData.Size.Y - 1 ? 1 : chunkData.Chunk[x, y, z + 1];

                    neighbours[5] = y == 0 ? 1 : chunkData.Chunk[x, y - 1, z];
                    neighbours[4] = y == chunkData.Height - 1 ? 1 : chunkData.Chunk[x, y + 1, z];

                    if (chunkData.Chunk[x, y, z] == 1)
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
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexPositionTexture[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
        sideVertexes[0].TextureCoordinate = new Vector2(1, 1);

        sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = new Vector2(1, 0);

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, 0f) + offSet;
        sideVertexes[2].TextureCoordinate = new Vector2(0, 0);

        sideVertexes[3].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, 0f) + offSet;
        sideVertexes[3].TextureCoordinate = new Vector2(0, 1);

        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
    }

    private void AddFrontSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet)
    {
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexPositionTexture[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, _voxelSize) + offSet;
        sideVertexes[0].TextureCoordinate = new Vector2(0, 1);

        sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, _voxelSize, _voxelSize) + offSet;
        sideVertexes[1].TextureCoordinate = new Vector2(0, 0);

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = new Vector2(1, 0);

        sideVertexes[3].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, _voxelSize) + offSet;
        sideVertexes[3].TextureCoordinate = new Vector2(1, 1);

        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex));
    }

    private void AddBottomSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet)
    {
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexPositionTexture[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
        sideVertexes[0].TextureCoordinate = new Vector2(0, 1);

        sideVertexes[1].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = new Vector2(1, 1);

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = new Vector2(1, 0);

        sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, 0f, _voxelSize) + offSet;
        sideVertexes[3].TextureCoordinate = new Vector2(0, 0);


        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
    }

    private void AddTopSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet)
    {
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexPositionTexture[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, _voxelSize, 0f) + offSet;
        sideVertexes[0].TextureCoordinate = new Vector2(0, 0);

        sideVertexes[1].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = new Vector2(1, 0);

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = new Vector2(1, 1);

        sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, _voxelSize, _voxelSize) + offSet;
        sideVertexes[3].TextureCoordinate = new Vector2(0, 1);


        int[] indices = { 1, 2, 0, 0, 2, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex));
    }

    private void AddRightSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet)
    {
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexPositionTexture[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, _voxelSize) + offSet;
        sideVertexes[0].TextureCoordinate = new Vector2(0, 1);

        sideVertexes[1].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = new Vector2(1, 0);

        sideVertexes[2].Position = vectorOfPosition + new Vector3(_voxelSize, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = new Vector2(0, 0);

        sideVertexes[3].Position = vectorOfPosition + new Vector3(_voxelSize, 0f, 0f) + offSet;
        sideVertexes[3].TextureCoordinate = new Vector2(1, 1);

        int[] indices = { 0, 2, 1, 0, 1, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex));
    }

    private void AddLeftSide(List<VertexPositionTexture> vertices, List<int> indexes, int x, int y, int z,
        Vector3 offSet)
    {
        var startIndex = vertices.Count;
        var vectorOfPosition = new Vector3((x * _voxelSize), (y * _voxelSize), (z * _voxelSize));
        var sideVertexes = new VertexPositionTexture[4];
        sideVertexes[0].Position = vectorOfPosition + new Vector3(0f, 0f, _voxelSize) + offSet;
        sideVertexes[0].TextureCoordinate = new Vector2(1, 1);

        sideVertexes[1].Position = vectorOfPosition + new Vector3(0f, _voxelSize, 0f) + offSet;
        sideVertexes[1].TextureCoordinate = new Vector2(0, 0);

        sideVertexes[2].Position = vectorOfPosition + new Vector3(0f, _voxelSize, _voxelSize) + offSet;
        sideVertexes[2].TextureCoordinate = new Vector2(1, 0);

        sideVertexes[3].Position = vectorOfPosition + new Vector3(0f, 0f, 0f) + offSet;
        sideVertexes[3].TextureCoordinate = new Vector2(0, 1);

        int[] indices = { 0, 2, 1, 0, 1, 3 };

        vertices.AddRange(sideVertexes);
        indexes.AddRange(indices.Select(b => b + startIndex).Reverse());
    }

    public ChunkVisualsRequest EnequeVisualCreation(ChunkData chunkData)
    {
        var request = new ChunkVisualsRequest()
        {
            ChunkData = chunkData
        };
        _calculationQueue.Enqueue(request);
        return request;
    }
}

public class ChunkVisualsRequest
{
    public ChunkData ChunkData { get; set; }
    public ChunkVisualData VisualsData { get; set; }
    public bool IsCalculated { get; set; } = false;
}

public struct ChunkVisualData
{
    public List<VertexPositionTexture> Vertexes { get; set; }
    public List<int> Indexes { get; set; }
}