using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

namespace ZE.MechBattle
{
    public interface IUserCountDependentLRUPathsBuffer<UserKey>
    {
        int PathsCount { get; }
        IReadOnlyDictionary<UserKey, int> UserToPathId { get; }
        void OnPathStartUse(UserKey userId, int pathId);
        void OnPathUserLeft(UserKey user);

        IBufferTrimController CreateTrimController();
    }

    // stores path while they are in use, trim by TrimController external calls (without losing active data)
    public abstract class UserCountDependentLRUPathsBuffer<UserKey, NodeKey> 
        : IUserCountDependentLRUPathsBuffer<UserKey>,IPathsList<NodeKey> ,
          ITrimmableBuffer<UserKey,NodeKey>,
          IEnumerable<PathData<NodeKey>>
        where NodeKey : unmanaged
    {
        public int PathDataVersion { get; private set; }
        public int PathsCount => _paths.Count;
        public IReadOnlyDictionary<UserKey, int> UserToPathId => _userToPathId;
        public IReadOnlyDictionary<int, PathData<NodeKey>> Paths => _paths;

        protected IReadOnlyDictionary<(NodeKey, NodeKey), int> DestinationsToPathId => _endpointsToPathId;        

        private readonly Dictionary<int, PathData<NodeKey>> _paths = new();
        private readonly Dictionary<(NodeKey, NodeKey), int> _endpointsToPathId = new();

        // note:
        // - why don't store users inside the path?
        // - because of clearing users is done when they haven't got path components (no need to check in every remove system, only in PathAccountingSystem)
        // However we simple store userId-pathId pairs and can count path users anytime
        private readonly Dictionary<UserKey, int> _userToPathId = new();

        private int _nextPathId = 1;

        public void UpdatePathDataVersion() => PathDataVersion++;

        public IBufferTrimController CreateTrimController() => new BufferTrimController<UserKey, NodeKey>(this);
        public bool IsPathExists(int pathId) => _paths.ContainsKey(pathId);
        public bool TryGetPath(int pathId, out PathData<NodeKey> data) => _paths.TryGetValue(pathId, out data);
        public bool TryGetPathByEndpoints(NodeKey start, NodeKey end, out PathData<NodeKey> pathData)
        {
            if (!DestinationsToPathId.TryGetValue(new(start, end), out var pathId))
            {
                pathData = default;
                return false;
            }

            if (TryGetPath(pathId, out pathData))
            {
                return true;
            }
            else
            {
                Debug.LogError("destinations dictionary and paths dictionary mismatch");
                return false;
            }
        }


        public void OnPathStartUse(UserKey userId, int pathId)
        {
            _userToPathId[userId] = pathId;
            _paths[pathId].UpdateUseTime();
        }

        public void OnPathUserLeft(UserKey user) => _userToPathId.Remove(user);

        public PathData<NodeKey> ReservePath((NodeKey, NodeKey) destinationKey)
        {
            var pathId = _nextPathId++;
            var path = new PathData<NodeKey>(pathId, destinationKey);
            _paths.Add(pathId, path);
            _endpointsToPathId.Add(destinationKey, pathId);
            PathDataVersion++;
            return path;
        }

        public void RemovePath(int pathId)
        {
            if (!_paths.TryGetValue(pathId, out var path))
                return;

            _paths.Remove(pathId);
            _endpointsToPathId.Remove(path.DestinationKey);

            PathDataVersion++;
        } 

        public PathData<NodeKey> AddCalculatedPath(int pathId, PathCalculationResult<NodeKey> calculatedData)
        {
            if (!_paths.TryGetValue(pathId, out var path))
                path = ReservePath(calculatedData.RequestedDestination);

            path.OnCalculationFinished(calculatedData);      
            PathDataVersion++;
            return path;
        }

        #region IEnumerable
        public IEnumerator<PathData<NodeKey>> GetEnumerator()
        {
            foreach (var path in _paths.Values)
            {
                yield return path;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        #endregion
    }
}
