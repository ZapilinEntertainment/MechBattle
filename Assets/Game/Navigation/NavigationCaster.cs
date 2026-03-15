using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    // protection from user-sent dispose requests
    public interface INavigationCaster
    {
        float TriangleEdgeSize { get; }

        // every navigation triangle will be subdivided into this count of triangles and every center will be raycasted
        int RaycastResolution { get; } 
        int HexTrianglesCount { get;}
        int TrianglesPerHexEdge { get; }
        Awaitable<NativeArray<RaycastHit>> CastHexAsync(float2 hexWorldPos, QueryParameters queryParameters, CancellationToken token);
    }

    public class NavigationCaster : IDisposable, INavigationCaster
    {
        public float TriangleEdgeSize => _triangleEdgeSize;
        public int RaycastResolution => _raycastTrianglesPerEdge;
        public int HexTrianglesCount => _hexTrianglesCount;
        public int TrianglesPerHexEdge => _trianglesPerHexEdge;

        private readonly int _trianglesPerHexEdge;
        private readonly int _raycastTrianglesPerEdge;
        private readonly int _hexTrianglesCount;
        private readonly float _triangleEdgeSize;
        private readonly float _castingHeight;
        private readonly float _castingRayLength;

        private readonly NativeArray<IntTriangularPos> _positionsArray;
        private readonly NativeArray<float2> _raycastPointsArray;

        // final commands list
        private readonly NativeList<RaycastCommand> _raycastCommands;

        public NavigationCaster(in MapSettings mapSettings, Allocator allocator) 
        { 
            _trianglesPerHexEdge = mapSettings.TrianglesPerHexEdge;
            _raycastTrianglesPerEdge = mapSettings.RaycastSubdivisionsPerEdge;
            _triangleEdgeSize = mapSettings.TriangleEdgeSize;
            _castingHeight = NavigationConstants.CASTING_HEIGHT;
            _castingRayLength = NavigationConstants.CASTING_RAY_LENGTH;

            _hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(mapSettings.TrianglesPerHexEdge);
            var raycastCommandsCount = _hexTrianglesCount * _raycastTrianglesPerEdge * _raycastTrianglesPerEdge;
            _positionsArray = new NativeArray<IntTriangularPos>(_hexTrianglesCount, allocator, NativeArrayOptions.UninitializedMemory);
            _raycastCommands = new NativeList<RaycastCommand>(raycastCommandsCount, allocator);

            var raycastsCount = _raycastTrianglesPerEdge * _raycastTrianglesPerEdge;
            _raycastPointsArray = new NativeArray<float2>(raycastsCount, allocator, NativeArrayOptions.UninitializedMemory);
        }

        public async Awaitable<NativeArray<RaycastHit>> CastHexAsync(float2 hexWorldPos, QueryParameters queryParameters, CancellationToken token)
        {
            var preparePositionsJob = new PrepareHexRaycastCommandsJob()
            {
                CastingHeight = _castingHeight,
                CastingRayLength = _castingRayLength,
                HexCenter = hexWorldPos,
                RaycastCommands = _raycastCommands,
                Positions = _positionsArray,
                QueryParameters = queryParameters,
                RaycastPoints = _raycastPointsArray,
                RaycastTrianglesPerEdge = _raycastTrianglesPerEdge,
                TriangleEdgeSize = _triangleEdgeSize,
                TrianglesPerHexEdge = _trianglesPerHexEdge,
            };

            var preparePositionsHandle = preparePositionsJob.ScheduleByRef();
            var raycastResults = new NativeArray<RaycastHit>(_raycastCommands.Length, Allocator.Persistent);
            var castJobHandle = RaycastCommand.ScheduleBatch(_raycastCommands.AsArray(), raycastResults, 16, dependsOn: preparePositionsHandle);

            while (!castJobHandle.IsCompleted && !token.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(token);
            }

            if (token.IsCancellationRequested)
            {
                raycastResults.Dispose();
                return default;
            }

            castJobHandle.Complete();
            return raycastResults;
        }


        public void Dispose()
        {
            _positionsArray.Dispose();
            _raycastCommands.Dispose();
            _raycastPointsArray.Dispose();
        }
    }
}
