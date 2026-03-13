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
        private readonly Dictionary<int4, int> _pathsByEdgePoints = new();
        private readonly HashSet<int4> _requestedPaths = new();
        private int _nextId = 1;
    
        public bool TryGetPathId(int2 start, int2 end, out int pathId) => _pathsByEdgePoints.TryGetValue(new(start, end), out pathId);
        public bool TryGetPathId(int4 edges, out int pathId) => _pathsByEdgePoints.TryGetValue(edges, out pathId);
        public void RequestPathBuilding(int2 start, int2 end) => _requestedPaths.Add(new(start, end));

        public void AddCalculatedPath(HexPath path)
        {
            var startEnd= new int4(path.Start, path.End);
            _requestedPaths.Remove(startEnd);
            
            var id = _nextId++;
            _pathsByEdgePoints.Add(startEnd, id);
            _pathsById.Add(id, path);
        }

        // for multiple calculations per frame
        public bool TryGetRequestedPaths(int maxCount, out int4[] paths)
        {
            if (_requestedPaths.Count == 0)
            {
                paths = null;
                return false;
            }

            maxCount = math.max(maxCount, _requestedPaths.Count);
            paths = new int4[maxCount];
            var i = 0;
            foreach (var requestedPath in _requestedPaths)
            {
                paths[i++] = requestedPath;
                if (i >= maxCount) 
                    break;
            }
            return true;
        }

        public bool TryGetRequestedPath(out int4 startEnd)
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
