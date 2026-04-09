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
        float TriangleHeight { get; }

        // every navigation triangle will be subdivided into this count of triangles and every center will be raycasted
        int RaycastResolution { get; } 
        int HexTrianglesCount { get;}
        int TrianglesPerHexEdge { get; }
        float HexEdgeSize { get; }
        NativeArray<RaycastHit> CastHex(
             Allocator allocator,
             float2 hexWorldPos,
             QueryParameters queryParameters);
        Awaitable<NativeArray<RaycastHit>> CastHexAsync(
            Allocator allocator, 
            float2 hexWorldPos, 
            QueryParameters queryParameters, 
            CancellationToken token);
    }

    public class NavigationCaster : IDisposable, INavigationCaster
    {
        public float TriangleHeight => _triangleHeight;
        public int RaycastResolution => _raycastTrianglesPerEdge;
        public int HexTrianglesCount => _hexTrianglesCount;
        public int TrianglesPerHexEdge => _trianglesPerHexEdge;
        public float HexEdgeSize => _hexEdgeSize;

        private readonly int _trianglesPerHexEdge;
        private readonly int _raycastTrianglesPerEdge;
        private readonly int _hexTrianglesCount;
        private readonly float _triangleHeight;
        private readonly float _castingHeight;
        private readonly float _castingRayLength;
        private readonly float _hexEdgeSize;

        private readonly NativeArray<IntTriangularPos> _positionsArray;
        private readonly NativeArray<SubdivideTriangleIntoSmallerOnesCommand.SmallTriangleData> _raycastPointsArray;

        // final commands list
        private readonly NativeArray<RaycastCommand> _raycastCommands;
        private readonly CancellationTokenSource _casterLifetimeCts = new();

        private bool _isDisposed = false;

        public NavigationCaster(MapSettingsSO mapSettings, Allocator allocator) 
        { 
            _trianglesPerHexEdge = mapSettings.TrianglesPerHexEdge;
            _raycastTrianglesPerEdge = mapSettings.RaycastSubdivisionsPerEdge;
            _triangleHeight = mapSettings.TriangleHeight;
            _castingHeight = NavigationConstants.CASTING_HEIGHT;
            _castingRayLength = NavigationConstants.CASTING_RAY_LENGTH;
            _hexEdgeSize = mapSettings.HexEdgeSize;

            _hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(mapSettings.TrianglesPerHexEdge);
            var raycastCommandsCount = _hexTrianglesCount * _raycastTrianglesPerEdge * _raycastTrianglesPerEdge;
            _positionsArray = new NativeArray<IntTriangularPos>(_hexTrianglesCount, allocator, NativeArrayOptions.UninitializedMemory);
            _raycastCommands = new NativeArray<RaycastCommand>(raycastCommandsCount, allocator);

            var raycastsCount = _raycastTrianglesPerEdge * _raycastTrianglesPerEdge;
            _raycastPointsArray = new (raycastsCount, allocator, NativeArrayOptions.UninitializedMemory);
        }

        public PrepareHexRaycastCommandsJob ConstructPositionsJob(float2 hexWorldPos, in QueryParameters queryParameters)
        {
            return new PrepareHexRaycastCommandsJob()
            {
                CastingHeight = _castingHeight,
                CastingRayLength = _castingRayLength,
                HexCenterWorld = hexWorldPos,
                RaycastCommands = _raycastCommands,
                Positions = _positionsArray,
                QueryParameters = queryParameters,
                RaycastPoints = _raycastPointsArray,
                RaycastTrianglesPerEdge = _raycastTrianglesPerEdge,
                TriangleHeight = _triangleHeight,
                TrianglesPerHexEdge = _trianglesPerHexEdge,
            };
        }

        public async Awaitable<NativeArray<RaycastHit>> CastHexAsync(
            Allocator allocator,
            float2 hexWorldPos,
            QueryParameters queryParameters, 
            CancellationToken cancellationToken)
        {
            if (_isDisposed)
                throw new Exception("Caster disposed");

            var castJobHandle = PrepareCastJob(allocator, hexWorldPos, queryParameters, out var raycastResults);

            var ownToken = _casterLifetimeCts.Token;
            while (!castJobHandle.IsCompleted)
            {
                await Awaitable.NextFrameAsync();
            }
            castJobHandle.Complete();

            if (cancellationToken.IsCancellationRequested || ownToken.IsCancellationRequested)
            {
                Debug.LogWarning("Casting was cancelled before raycast job complete");
                return raycastResults;
            }
            else
            {
                return raycastResults;
            }
        }

        public NativeArray<RaycastHit> CastHex(
             Allocator allocator, 
             float2 hexWorldPos,
             QueryParameters queryParameters)
        {
            if (_isDisposed)
                throw new Exception("Caster disposed");

            var castJobHandle = PrepareCastJob(allocator, hexWorldPos, queryParameters, out var raycastResults);
            castJobHandle.Complete();
            return raycastResults;
        }

        private JobHandle PrepareCastJob(
            Allocator allocator, 
            float2 hexWorldPos, 
            QueryParameters queryParameters, 
            out NativeArray<RaycastHit> raycastResults)
        {
            var positionsJob = ConstructPositionsJob(hexWorldPos, queryParameters);

            var preparePositionsHandle = positionsJob.ScheduleByRef();
            raycastResults = new NativeArray<RaycastHit>(_raycastCommands.Length, allocator);
            return RaycastCommand.ScheduleBatch(_raycastCommands, raycastResults, 64, dependsOn: preparePositionsHandle);
        }


        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _casterLifetimeCts.Cancel();
            _casterLifetimeCts.Dispose();

            _positionsArray.Dispose();
            _raycastCommands.Dispose();
            _raycastPointsArray.Dispose();
        }
    }
}
