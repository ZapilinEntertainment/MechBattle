using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class FlattenHexTrianglesCommand
    {
        public static SquaredHexTrianglesList<FlowFieldCellSetupData> PrepareFlowMapSetupData(INavigationMap map, NavigationHex hex, Allocator allocator)
        {
            var trianglesCount = TriangularMath.GetTrianglesCountInHex(map.TrianglesPerHexEdge);
            var flattenedTrianglesList = new SquaredHexTrianglesList<FlowFieldCellSetupData>(hex.TriangularCenterPos, map.TrianglesPerHexEdge, allocator);

            using (var positionsList = new NativeArray<IntTriangularPos>(trianglesCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory))
            {
                GetTrianglesInHexCommand.Execute(hex.InnerRingTopTrianglePos, map.TrianglesPerHexEdge, positionsList);
                var converter = flattenedTrianglesList.CoordsConverter;
                foreach (var trianglePos in positionsList)
                {
                    flattenedTrianglesList.Set(trianglePos, new()
                    {
                        EntranceCost = map.GetTriangleEntranceCost(trianglePos),
                        IsValid = true
                    });
                }
            }

            return flattenedTrianglesList;
        }
    
    }
}
