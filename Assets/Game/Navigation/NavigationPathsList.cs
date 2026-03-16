using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public class NavigationPathsList
    {
        // TODO: add unsude paths clear mechanism

        private readonly Dictionary<int, HexPath> _pathsById = new();
        private readonly Dictionary<HexPathKey, int> _pathsByEdgePoints = new();
        private readonly HashSet<HexPathKey> _requestedPaths = new();
        private int _nextId = 1;
    
        public bool TryGetPathId(HexPathNodeKey start, HexPathNodeKey end, out int pathId) => _pathsByEdgePoints.TryGetValue(new(start, end), out pathId);
        public bool TryGetPathId(HexPathKey edges, out int pathId) => _pathsByEdgePoints.TryGetValue(edges, out pathId);
        public void RequestPathBuilding(HexPathNodeKey start, HexPathNodeKey end) => _requestedPaths.Add(new(start, end));

        public void AddCalculatedPath(HexPath path)
        {
            var pathKey= new HexPathKey(path.Start, path.End);
            _requestedPaths.Remove(pathKey);
            
            var id = _nextId++;
            _pathsByEdgePoints.Add(pathKey, id);
            _pathsById.Add(id, path);
        }

        // for multiple calculations per frame
        public bool TryGetRequestedPaths(int maxCount, out HexPathKey[] paths)
        {
            if (_requestedPaths.Count == 0)
            {
                paths = null;
                return false;
            }

            maxCount = math.max(maxCount, _requestedPaths.Count);
            paths = new HexPathKey[maxCount];
            var i = 0;
            foreach (var requestedPath in _requestedPaths)
            {
                paths[i++] = requestedPath;
                if (i >= maxCount) 
                    break;
            }
            return true;
        }

        public bool TryGetRequestedPath(out HexPathKey startEnd)
        {
            foreach (var path in _requestedPaths)
            {
                startEnd = path;
                return true;
            }

            startEnd = default;
            return false;
        }
    }
}
