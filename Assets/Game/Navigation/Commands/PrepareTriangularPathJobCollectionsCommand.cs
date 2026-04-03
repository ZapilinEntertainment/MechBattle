using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class PrepareTriangularPathJobCollectionsCommand
    {
        public static TriangularPathJobCollections Execute(Allocator allocator, NavigationHexPosition hexPos, int hexRadius, IFlowMap flowMap )
        {
            var data = new TriangularPathJobCollections(allocator, hexPos, hexRadius);            
            var coordsConverter = data.SetupData.CoordsConverter;

            foreach (var tripos in new HexTrianglesEnumerator(hexPos, hexRadius))
            {
                var cell = flowMap.GetCombinedCellData(tripos);
                data.SetupData.Set(tripos, cell.TriangleData);

                var index = coordsConverter.TriangularToIndex(tripos);
                var calcData = data.CalculationData;
                calcData[index] = new AstarPathNodeData<IntTriangularPos>(tripos);
            }

            return data;
        }
    }
}
