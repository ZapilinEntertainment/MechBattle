using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation
{
    public static class PrepareNavigationMapCommand
    {
        public static NavigationMap Execute(in MapSettings settings, Allocator allocator)
        {
            var map = new NavigationMap(settings, allocator);
            Execute(map);
            return map;
        }

        public static void Execute(IUpdatableMap map)
        {
            CastMap(map);
            using var defineCollections = new DefineTransitionTrianglesJobCollection(Allocator.TempJob);
            UpdateHexEdgesPassabilityCommand.Execute(map, defineCollections);
            map.OnInitialized();
        }

        public static void CastMap(IUpdatableMap map) 
        {
            var allocator = Allocator.TempJob;
            var settings = map.Settings;
            using var hexes = GetHexesInRectangleCommand.Execute(settings, allocator);

            using var mapUpdater = new MapUpdater(allocator, map);            
            var mapResourcesAllocator = map.ResourcesAllocator;
            for (var i = 0; i < hexes.Length; i++)
            {
                var hexCoord = hexes[i];
                mapUpdater.UpdateHex(hexCoord);
            }
        }
    
    }
}
