using Scellecs.Morpeh;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    // stores path while they are in use
    public class NavigationTrianglePathsBuffer
    {
        public struct TrianglePathData : IDisposable
        {
            public readonly IntTriangularPos[] Points;
            public readonly int Length;
            public float LastUseTime;

            public int6 GetDestinationKey() => new(Points[0], Points[Length - 1]);

            public TrianglePathData(in NativeArray<IntTriangularPos> readList)
            {
                Length = readList.Length;
                Points = ArrayPool<IntTriangularPos>.Shared.Rent(Length);
                for (var i = 0; i < Length; i++)
                {
                    Points[i] = readList[i];
                }
                LastUseTime = Time.time;
            }

            public void Dispose()
            {
                ArrayPool<IntTriangularPos>.Shared.Return(Points);
            }

            public bool TryGetTriangle(int stepIndex, out IntTriangularPos pos)
            {
                if (stepIndex < 0 || stepIndex >= Length)
                {
                    pos = default;
                    return false;
                }
                
                pos = Points[stepIndex];
                return true;
            }
        }

        public class BufferClearController
        {
            private readonly Dictionary<int,int> _pathUsersCount = new();
            private readonly NavigationTrianglePathsBuffer _buffer;

            public BufferClearController(NavigationTrianglePathsBuffer buffer)
            {
                _buffer = buffer;
            }

            public void Execute(int limit)
            {
                foreach (var userToPathKvp in _buffer._userToPathId)
                {
                    _pathUsersCount[userToPathKvp.Value]++;
                }

                var candidatesToRemove = _buffer._paths
                    .Where(p => !_pathUsersCount.ContainsKey(p.Key)) 
                    .OrderByDescending(p => p.Value.LastUseTime)  
                    .Select(p => p.Key);

                var count = _buffer._paths.Count;
                foreach (var pathId in candidatesToRemove)
                {
                    _buffer.RemovePath(pathId);
                    count--;
                    if (count <= limit)
                        break;
                }

                _pathUsersCount.Clear();
            }
        }

        public int PathsCount => _paths.Count;
        public IReadOnlyDictionary<Entity, int> UserToPathId => _userToPathId;

        private readonly Dictionary<int, TrianglePathData> _paths = new();
        private readonly Dictionary<int6, int> _destinationsToPathId = new();
        private readonly Dictionary<Entity, int> _userToPathId = new();

        private int _nextPathId = 1;


        public BufferClearController CreateClearController() => new(this);
        public bool IsPathExists(int pathId) => _paths.ContainsKey(pathId);
        public bool TryGetPath(int pathId, out TrianglePathData data) => _paths.TryGetValue(pathId, out data);

        public bool TryGetPathShortData(IntTriangularPos start, IntTriangularPos end, out TrianglePathShortData shortPathData)
        {
            if (!_destinationsToPathId.TryGetValue(new(start, end), out var pathId))
            {
                shortPathData = default;
                return false;
            }

            if (TryGetPathShortData(pathId, out shortPathData))
            {
                return true;
            }
            else
            {
                Debug.LogError("destinations dictionary and paths dictionary mismatch");
                return false;
            }
        }

        public bool TryGetPathShortData(int pathId, out TrianglePathShortData shortPathData)
        {
            if (!_paths.TryGetValue(pathId, out var pathData))
            {
                shortPathData = default;
                return false;
            }

            shortPathData = new(pathId: pathId, trianglesCount: pathData.Length);
            return true;
        }


        public void OnPathStartUse(Entity userId, int pathId) 
        {
            _userToPathId[userId] = pathId;

            var pathData = _paths[pathId];
            pathData.LastUseTime = Time.time;
            _paths[pathId] = pathData;
        }

        public void OnPathUserLeft(Entity user) => _userToPathId.Remove(user);

        public int RegisterNewPath(in NativeArray<IntTriangularPos> positions)
        {
            var pathId = _nextPathId++;
            FulfillReservedPath(pathId, positions);
            return pathId;
        }

        public int ReservePathId() => _nextPathId++;
        public void FulfillReservedPath(int pathId, in NativeArray<IntTriangularPos> positions)
        {
#if UNITY_EDITOR
            if (_paths.ContainsKey(pathId))
                Debug.LogError("reserved path place is already occupied");
#endif

            if (positions.Length < 2)
            {
                Debug.LogError($"invalid path {pathId} with {positions.Length} nodes");
                return;
            }

            var pathData = new TrianglePathData(positions);
            _paths[pathId] = pathData;
            _destinationsToPathId[pathData.GetDestinationKey()] = pathId;
        }

        private void RemovePath(int pathId)
        {
            var path = _paths[pathId];
            var destinationKey = path.GetDestinationKey();
            path.Dispose();

            _paths.Remove(pathId);
            _destinationsToPathId.Remove(destinationKey);
        }
    }
}
