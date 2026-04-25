using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class PrepareTriangularPathJobCollectionsCommand
    {
        public static TriangularPathJobCollections Execute(
            Allocator allocator,
            NavigationHexPosition hexPos, 
            INavigationMap map)
        {
            var mapSettings = map.Settings;
            var data = new TriangularPathJobCollections(allocator, hexPos, mapSettings);            
            ref var setupData = ref data.PassabilityData;

            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, mapSettings.TrianglesPerHexEdge))
            {
                setupData[tripos] = map.GetPassabilityData(tripos);

                var index = setupData.TriangularToIndex(tripos);
                var calcData = data.CalculationData;
                calcData[index] = new AstarPathNodeData<IntTriangularPos>(tripos);
            }

            return data;
        }
    }
}
