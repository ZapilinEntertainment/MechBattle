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
            CastMap(map);
            return map;
        }

        public static void CastMap(INavigationMap map) 
        {
            var allocator = Allocator.TempJob;
            var settings = map.Settings;
            using var hexes = GetHexesInRectangleCommand.Execute(settings, allocator);

            using var flowMapFactory = new FlowMapFactory(allocator, settings);            
            var mapResourcesAllocator = map.ResourcesAllocator;
            for (var i = 0; i < hexes.Length; i++)
            {
                var hexCoord = hexes[i];
                var flowMap = flowMapFactory.CreateHexFlowMap(mapResourcesAllocator, hexCoord);
                map.UpdateHexFlowMap(hexCoord, flowMap);
            }
        }
    
    }
}
