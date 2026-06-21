using System.Collections.Generic;
using UnityEngine;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public abstract class UserCountDependentLRUPathsBuffer<DestinationKey, NodeKey, PathType> 
        : UseTimeStoringDictionary<int, PathType>,
          IPathsList<DestinationKey, NodeKey>         
        where PathType : PathData<DestinationKey, NodeKey>
        where NodeKey : unmanaged
        where DestinationKey : unmanaged
    {

        protected IReadOnlyDictionary<(DestinationKey, DestinationKey), int> DestinationsToPathId => _endpointsToPathId;        
        private readonly Dictionary<(DestinationKey, DestinationKey), int> _endpointsToPathId = new();

        private int _nextPathId = 1;
        public bool TryGetPathByEndpoints(DestinationKey start, DestinationKey end, out PathType pathData, bool updateUsingTime)
        {
            if (!DestinationsToPathId.TryGetValue(new(start, end), out var pathId))
            {
                pathData = default;
                return false;
            }

            if (TryGetValue(pathId, out pathData, updateUsingTime))
            {
                return true;
            }
            else
            {
                Debug.LogError("destinations dictionary and paths dictionary mismatch");
                return false;
            }
        }
      

        public PathType ReservePath(DestinationKey start, DestinationKey end)
        {
            var pathId = _nextPathId++;
            var path = CreateNewPath(pathId, start, end);
            _endpointsToPathId.Add((start,end), pathId);
            Add(pathId, path);           
            return path;
        }
       

        public PathType AddCalculatedPath(int pathId, PathCalculationResult<DestinationKey, NodeKey> calculatedData)
        {
            if (!TryGetValue(pathId, out var path, true))
                path = ReservePath(calculatedData.Start, calculatedData.End);

            path.OnCalculationFinished(calculatedData);      
            UpdateVersion();
            return path;
        }

        protected override void OnElementRemoved(int key, PathType value)
        {
            _endpointsToPathId.Remove(value.DestinationKeys);
        }  

        protected abstract PathType CreateNewPath(int pathId, DestinationKey start, DestinationKey end);

        void IPathsList<DestinationKey, NodeKey>.AddCalculatedPath(int pathKey, PathCalculationResult<DestinationKey, NodeKey> calculatedData) =>
            AddCalculatedPath(pathKey, calculatedData);

        int IPathsList<DestinationKey, NodeKey>.ReservePath(DestinationKey start, DestinationKey end) =>
            ReservePath(start, end).Id;
    }
}
