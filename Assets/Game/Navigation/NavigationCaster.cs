using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

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
    }

    // support class for making casts
    public class NavigationCaster : INavigationCaster, IDisposable
    {

        public float TriangleHeight => _triangleHeight;
        public int RaycastResolution => _raycastTrianglesPerEdge;
        public int HexTrianglesCount => _hexTrianglesCount;
        public int TrianglesPerHexEdge => _trianglesPerHexEdge;
        public float HexEdgeSize => _hexEdgeSize;
        public readonly int ResultsLength;

        private readonly int _trianglesPerHexEdge;
        private readonly int _raycastTrianglesPerEdge;
        private readonly int _hexTrianglesCount;
        private readonly float _triangleHeight;
        private readonly float _castingHeight;
        private readonly float _castingRayLength;
        private readonly float _hexEdgeSize;
        private readonly NativeArray<SmallTriangleData> _raycastPointsArray;
        private readonly QueryParameters _queryParameters;

        // final commands list
        private readonly NativeArray<RaycastCommand> _raycastCommands;
        private readonly NativeArray<RaycastHit> _raycastResults;
        private readonly CancellationTokenSource _casterLifetimeCts = new();

        private bool _isDisposed = false;
        private JobHandle _activeJobHandle;

        public NavigationCaster(Allocator allocator, MapSettings mapSettings, QueryParameters queryParameters) 
        { 
            _trianglesPerHexEdge = mapSettings.TrianglesPerHexEdge;
            _raycastTrianglesPerEdge = mapSettings.RaycastSubdivisionsPerEdge;
            _triangleHeight = mapSettings.TriangleHeight;
            _castingHeight = NavigationConstants.CASTING_HEIGHT;
            _castingRayLength = NavigationConstants.CASTING_RAY_LENGTH;
            _hexEdgeSize = mapSettings.HexEdgeSize;
            _queryParameters = queryParameters;

            _hexTrianglesCount = mapSettings.TrianglesCountInHex;
            ResultsLength = mapSettings.RaycastsPerHex;
            _raycastCommands = new NativeArray<RaycastCommand>(ResultsLength, allocator);
            _raycastResults = new NativeArray<RaycastHit>(ResultsLength, allocator);

            var raycastsCountPerTriangle = _raycastTrianglesPerEdge * _raycastTrianglesPerEdge;
            _raycastPointsArray = new (raycastsCountPerTriangle, allocator, NativeArrayOptions.UninitializedMemory);
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

        public void CastHex(NavigationHexPosition hexPos)
        {
            if (_isDisposed)
                throw new Exception("Caster disposed");

            var castJobHandle = ScheduleCastJob(hexPos);
            castJobHandle.Complete();
        }

        public JobHandle ScheduleCastJob(NavigationHexPosition hexPos)
        {
            if (!_activeJobHandle.IsCompleted)
                throw new Exception("caster is still busy");

            //UnityEngine.Debug.Log($"start casting {hexPos.HexCoordinate}");
            var positionsJob = ConstructPositionsJob(hexPos, _trianglesPerHexEdge);           
            var positionsHandle = positionsJob.Schedule();
            //
            var raycastHandle = RaycastCommand.ScheduleBatch(_raycastCommands, _raycastResults, 64,  dependsOn: positionsHandle);
            _activeJobHandle = raycastHandle;
            return _activeJobHandle;
        }

        //
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _casterLifetimeCts.Cancel();
            _casterLifetimeCts.Dispose();

            _raycastCommands.Dispose();
            _raycastPointsArray.Dispose();
            _raycastResults.Dispose();
        }


        public void GetResults(RaycastHit[] receiverArray)
        {
            // COMPLETE REQUIRED:
            _activeJobHandle.Complete();
            for (var i = 0; i < _raycastResults.Length; i++)
            {
                receiverArray[i] = _raycastResults[i];
            }
        }
    }
}
