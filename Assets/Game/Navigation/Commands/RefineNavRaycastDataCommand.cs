using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    // refines raycast data into triangles navigation data (containing meaning ful info: is obstacled, heights, etc)
    public static class RefineNavRaycastDataCommand
    {

        private struct TriangleRaycastData
        {
            public int ObstacledCellsCount;

            public int GroundCastsCount;
            public float AverageGroundHeight;

            public short GetResultingAverageHeight() => (short)math.round(AverageGroundHeight);
        }

        public static NativeHashMap<IntTriangularPos, TriangleNavData> Execute(
            Allocator allocator,
           NavigationHexPosition hexPos,
           NativeArray<RaycastHit>.ReadOnly walkableHits,
           NativeArray<RaycastHit>.ReadOnly obstacleHits,
           in MapSettings settings)
        {
            var triangleHeight = settings.TriangleHeight;
            var raycastsPerTriangle = (float)(settings.RaycastSubdivisionsPerEdge * settings.RaycastSubdivisionsPerEdge);
            var intersectionPercentForLock = settings.IntersectionPercentForLock;

            return default;
            //var raycastData = new NativeHashMap<IntTriangularPos, TriangleRaycastData>();
            //foreach (var i)

            //for (var i = 0; i < walkableHits.Length; i++)
            //{
            //    raycastData.TryGetValue(trianglePos, out var data);

            //    var result = walkableHits[i];
            //    if (result.colliderInstanceID != 0)
            //    {
                    

            //        data.AverageGroundHeight = (data.AverageGroundHeight * data.GroundCastsCount + result.point.y) / (data.GroundCastsCount + 1);
            //        data.GroundCastsCount++;

            //        raycastData[trianglePos] = data;
            //    }
                
            //}

            //var trianglesData = new Dictionary<IntTriangularPos, TriangleNavData>();
            //foreach (var triKvp in raycastData)
            //{
            //    var data = triKvp.Value;
            //    var isLocked = (data.ObstaclesCount / raycastsPerTriangle) >= intersectionPercentForLock;
            //    trianglesData.Add(triKvp.Key, 
            //        new(
            //            isPassable: !isLocked, 
            //            height: data.GetResultingAverageHeight(), 
            //            entranceCost: NavigationConstants.DEFAULT_TRIANGLE_ENTRANCE_COST));
            //}

            //foreach (var tripos in new HexTrianglesEnumerator(hexPos, settings.TrianglesPerHexEdge))
            //{
            //    if (!trianglesData.ContainsKey(tripos))
            //        trianglesData.Add(tripos, new(false, NavigationConstants.DEFAULT_HEIGHT, sbyte.MaxValue));
            //}

            //return trianglesData;
        }
    
    }
}
