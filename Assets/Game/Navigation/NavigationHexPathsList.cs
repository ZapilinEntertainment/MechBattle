using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;


namespace ZE.MechBattle.Navigation
{
    public readonly struct PathShortData
    {
        public readonly int PathId;
        public readonly float PathLength;

        public PathShortData(int pathId, float pathLength)
        {
            PathId = pathId;
            PathLength = pathLength;
        }
    }

    public class NavigationHexPathsList
    {
        public int Version { get; private set; } = 0;
        // TODO: add unused paths clear mechanism

        // why use two keys (path key and int):  some paths can be outdated but valid (path exists, however new one is shorter)
        // so user of old path can end his path without changing route
        private readonly Dictionary<int, HexPath> _pathsById = new();
        private readonly Dictionary<HexPathKey, int> _pathsByEdgePoints = new();
        private readonly HashSet<HexPathKey> _requestedPaths = new();
        private readonly Dictionary<int2, HexEdgesMask> _calculatedHexPathsMask = new();
        private int _nextId = 1;
    
        public bool TryGetPathId(HexPathNodeKey start, HexPathNodeKey end, out int pathId) => _pathsByEdgePoints.TryGetValue(new(start, end), out pathId);
        public bool TryGetPathId(HexPathKey edges, out int pathId) => _pathsByEdgePoints.TryGetValue(edges, out pathId);
        public void RequestPathBuilding(HexPathNodeKey start, HexPathNodeKey end) => _requestedPaths.Add(new(start, end));

        public HexEdgesMask GetCalculatedEdgesMask(int2 hexCoord) => _calculatedHexPathsMask.TryGetValue(hexCoord, out var mask) ? mask : default;

        public void AddCalculatedPath(HexPath path)
        {
            var pathKey = new HexPathKey(path.Start, path.End);
            _requestedPaths.Remove(pathKey);
            
            var id = _nextId++;
            _pathsByEdgePoints.Add(pathKey, id);
            _pathsById.Add(id, path);

            OnHexEdgeCalculated(path.Start);
            OnHexEdgeCalculated(path.End);

            Version++;
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

        public bool TryGetPathShortData(HexPathNodeKey start, HexPathNodeKey end, out PathShortData path)
        {
            if (_pathsByEdgePoints.TryGetValue(new(start, end), out var pathId))
            {
                path = new PathShortData(pathId, _pathsById[pathId].Cost);
                return true;
            }

            path = default;
            return false;
        }

        private void OnHexEdgeCalculated(HexPathNodeKey key)
        {
            var newMask = _calculatedHexPathsMask.TryGetValue(key.HexCoord, out var existingMask) ? existingMask : default;
            _calculatedHexPathsMask[key.HexCoord] = newMask.SetEdgeStatus(key.Edge, true);
        }
    }
}
