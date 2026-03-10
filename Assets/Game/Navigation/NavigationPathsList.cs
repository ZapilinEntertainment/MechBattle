using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public class NavigationPathsList
    {
        public readonly struct HexPath
        {
            public readonly int2[] Points;
            public int2 Start => Points[0];
            public int2 End => Points[Points.Length - 1];
            public int4 EdgePoints => new(Start, End);

            public HexPath(int2[] pts) => Points = pts;
        }

        private readonly Dictionary<int, HexPath> _pathsById = new();
        private readonly Dictionary<int4, int> _pathsByEdgePoints = new();
        private readonly HashSet<int4> _requestedPaths = new();
    
        public bool TryGetPathId(int2 start, int2 end, out int pathId) => _pathsByEdgePoints.TryGetValue(new(start, end), out pathId);
        public bool TryGetPathId(int4 edges, out int pathId) => _pathsByEdgePoints.TryGetValue(edges, out pathId);
        public void RequestPathBuilding(int2 start, int2 end) => _requestedPaths.Add(new(start, end));

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
    }
}
