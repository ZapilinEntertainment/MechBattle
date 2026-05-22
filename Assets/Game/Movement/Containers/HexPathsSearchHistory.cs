using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class HexPathsSearchHistory
    {
        private readonly LRUDictionaryCache<CachedHexPathKey, int> _cachedSearchResults = new(MovementConstants.MAX_CACHED_HEX_PATHS);
        private readonly IHexPathsList _paths;

        // note: cached search results return path id
        // paths buffer contains paths by id and by destination (portal A id -> portal B id)

        [Inject]
        public HexPathsSearchHistory(HexPathsLRUBuffer hexPaths) 
        {
            _paths = hexPaths;
        }

        public bool TryGetCachedSolution(in HexPathSearchRequest request, out PathData<int> path)
        {
            var searchKey = RequestToKey(request);
            if (_cachedSearchResults.TryGetCachedValue(searchKey, out var cachedPathId))
            {
                if (_paths.TryGetPath(cachedPathId, out path))
                {
                    return true;                    
                }
                else
                {
                    _cachedSearchResults.Remove(searchKey);
                }
            }

            path = null;
            return false;
        }

        public void OnHexPathAdded(in HexPathSearchRequest request, int resultingPathId)
        {
            var key = RequestToKey(request);
            _cachedSearchResults.AddCachedValue(key, resultingPathId);
        }

        private CachedHexPathKey RequestToKey(in HexPathSearchRequest request) => 
            new (request.StartHexCoord, request.EndHexCoord, request.StartHexZoneIndex, request.EndHexZoneIndex);
    }
}
