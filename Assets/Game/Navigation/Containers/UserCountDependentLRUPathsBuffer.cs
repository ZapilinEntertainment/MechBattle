using System.Collections.Generic;
using UnityEngine;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public abstract class UserCountDependentLRUPathsBuffer<DestinationKey, NodeKey> 
        : UseTimeStoringDictionary<int, PathData<DestinationKey, NodeKey>>,
          IPathsList<DestinationKey, NodeKey>         
        where NodeKey : unmanaged
        where DestinationKey : unmanaged
    {

        protected IReadOnlyDictionary<(DestinationKey, DestinationKey), int> DestinationsToPathId => _endpointsToPathId;        
        private readonly Dictionary<(DestinationKey, DestinationKey), int> _endpointsToPathId = new();

        private int _nextPathId = 1;
        public bool TryGetPathByEndpoints(DestinationKey start, DestinationKey end, out PathData<DestinationKey, NodeKey> pathData, bool updateUsingTime)
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
      

        public PathData<DestinationKey, NodeKey> ReservePath(DestinationKey start, DestinationKey end)
        {
            var pathId = _nextPathId++;
            var destinationKey = (start, end);
            var path = new PathData<DestinationKey, NodeKey>(pathId, (start, end));
            _endpointsToPathId.Add(destinationKey, pathId);
            Add(pathId, path);
            return path;
        }
       

        public PathData<DestinationKey, NodeKey> AddCalculatedPath(int pathId, PathCalculationResult<DestinationKey, NodeKey> calculatedData)
        {
            if (!TryGetValue(pathId, out var path, true))
                path = ReservePath(calculatedData.Start, calculatedData.End);

            path.OnCalculationFinished(calculatedData);      
            UpdateVersion();
            return path;
        }

        protected override void OnElementRemoved(PathData<DestinationKey, NodeKey> path)
        {
            _endpointsToPathId.Remove(path.DestinationKeys);
        }       
    }
}
