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
        int HexTrianglesCount { get; }
        int TrianglesPerHexEdge { get; }
        float HexEdgeSize { get; }
        void CastHex(NavigationHexPosition hexPos);
        Awaitable CastHexAsync(
            NavigationHexPosition hexPos,
            CancellationToken cancellationToken);
    }

    // support class for making casts
    public class NavigationCaster : INavigationCaster, IDisposable
    {
        public float TriangleHeight => _triangleHeight;
        public int RaycastResolution => _raycastTrianglesPerEdge;
        public int HexTrianglesCount => _hexTrianglesCount;
        public int TrianglesPerHexEdge => _trianglesPerHexEdge;
        public float HexEdgeSize => _hexEdgeSize;
        public NativeArray<RaycastHit>.ReadOnly Results => _raycastResults.AsReadOnly();

        private readonly int _trianglesPerHexEdge;
        private readonly int _raycastTrianglesPerEdge;
        private readonly int _hexTrianglesCount;
        private readonly float _triangleHeight;
        private readonly float _castingHeight;
        private readonly float _castingRayLength;
        private readonly float _hexEdgeSize;
        private readonly NativeArray<SubdivideTriangleCommand.SmallTriangleData> _raycastPointsArray;
        private readonly QueryParameters _queryParameters;

        // final commands list
        private readonly NativeArray<RaycastCommand> _raycastCommands;
        private readonly NativeArray<RaycastHit> _raycastResults;
        private readonly CancellationTokenSource _casterLifetimeCts = new();

        private bool _isDisposed = false;

        public NavigationCaster(Allocator allocator, in MapSettings mapSettings, QueryParameters queryParameters) 
        { 
            _trianglesPerHexEdge = mapSettings.TrianglesPerHexEdge;
            _raycastTrianglesPerEdge = mapSettings.RaycastSubdivisionsPerEdge;
            _triangleHeight = mapSettings.TriangleHeight;
            _castingHeight = NavigationConstants.CASTING_HEIGHT;
            _castingRayLength = NavigationConstants.CASTING_RAY_LENGTH;
            _hexEdgeSize = mapSettings.HexEdgeSize;
            _queryParameters = queryParameters;

            _hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(mapSettings.TrianglesPerHexEdge);
            var raycastCommandsCount = _hexTrianglesCount * _raycastTrianglesPerEdge * _raycastTrianglesPerEdge;
            _raycastCommands = new NativeArray<RaycastCommand>(raycastCommandsCount, allocator);
            _raycastResults = new NativeArray<RaycastHit>(raycastCommandsCount, allocator);

            var raycastsCount = _raycastTrianglesPerEdge * _raycastTrianglesPerEdge;
            _raycastPointsArray = new (raycastsCount, allocator, NativeArrayOptions.UninitializedMemory);
        }

        public NavigationCaster(Allocator allocator, MapSettingsSO mapSettings, QueryParameters queryParameters ) : this(allocator, mapSettings.ToStruct(), queryParameters) { }

        public PrepareHexRaycastCommandsJob ConstructPositionsJob(NavigationHexPosition hexPos, int trianglesPerEdge)
        {
            return new PrepareHexRaycastCommandsJob()
            {
                CastingHeight = _castingHeight,
                CastingRayLength = _castingRayLength,
                RaycastCommands = _raycastCommands,
                QueryParameters = _queryParameters,
                RaycastPoints = _raycastPointsArray,
                RaycastTrianglesPerEdge = _raycastTrianglesPerEdge,
                TriangleHeight = _triangleHeight,
                HexPos = hexPos,
                TrianglesPerEdge = trianglesPerEdge
            };
        }

        public async Awaitable CastHexAsync(
            NavigationHexPosition hexPos,
            CancellationToken cancellationToken)
        {
            if (_isDisposed)
                throw new Exception("Caster disposed");

            var castJobHandle = ScheduleCastJob(hexPos);

            var ownToken = _casterLifetimeCts.Token;
            while (!castJobHandle.IsCompleted)
            {
                await Awaitable.NextFrameAsync();
            }
            castJobHandle.Complete();

            if (cancellationToken.IsCancellationRequested || ownToken.IsCancellationRequested)
            {
                Debug.LogWarning("Casting was cancelled before raycast job complete");
            }
        }

        public void CastHex(NavigationHexPosition hexPos)
        {
            if (_isDisposed)
                throw new Exception("Caster disposed");

            var castJobHandle = ScheduleCastJob(hexPos);
            castJobHandle.Complete();
        }

        public JobHandle ScheduleCastJob(NavigationHexPosition hexPos)
        {
            var positionsJob = ConstructPositionsJob(hexPos, _trianglesPerHexEdge);           
            var preparePositionsHandle = positionsJob.ScheduleByRef();
            return RaycastCommand.ScheduleBatch(_raycastCommands, _raycastResults, 64, dependsOn: preparePositionsHandle);
        }


        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _casterLifetimeCts.Cancel();
            _casterLifetimeCts.Dispose();

#if UNITY_EDITOR
            if (ZE.Utils.EditorPlaymodeLifetimeObject.IsQuitting)
                return;
#endif  
            _raycastCommands.Dispose();
            _raycastPointsArray.Dispose();
            _raycastResults.Dispose();
        }
    }
}
