using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    // refines raycast data into triangles navigation data
    public static class RefineNavRaycastDataCommand
    {
        private struct TriangleRaycastData
        {
            public int IntersectionsCount;
            public int ObstaclesCount;
            //public float MaxHeight;
            public float AverageHeight;
        }

        public static Dictionary<IntTriangularPos, NavigationTriangleData> Execute(NativeArray<RaycastHit>.ReadOnly raycastResults, float intersectionPercentForLock, INavigationCaster caster)
        {
            var intersectionsCount = new Dictionary<IntTriangularPos, TriangleRaycastData>();

            for (var i = 0; i < raycastResults.Length; i++)
            {
                var result = raycastResults[i];
                if (result.collider == null)
                    continue;

                var trianglePos = TriangularMath.WorldToTrianglePos(result.point, caster.TriangleEdgeSize);
                intersectionsCount.TryGetValue(trianglePos, out var data);

                data.AverageHeight = (data.AverageHeight * data.IntersectionsCount + result.point.y) / (data.IntersectionsCount + 1);
                data.IntersectionsCount++;
                //data.MaxHeight = math.max(data.MaxHeight, result.point.y);
                
                if (result.collider.CompareTag(NavigationConstants.OBSTACLE_TAG))
                    data.ObstaclesCount++;

                intersectionsCount[trianglePos] = data;
            }

            var trianglesData = new Dictionary<IntTriangularPos, NavigationTriangleData>();
            var raycastsPerTriangle = (float)(caster.RaycastResolution * caster.RaycastResolution);
            foreach (var triKvp in intersectionsCount)
            {
                var data = triKvp.Value;
                var isLocked = (data.ObstaclesCount / raycastsPerTriangle) >= intersectionPercentForLock;
                trianglesData.Add(triKvp.Key, new()
                {
                    Height = data.AverageHeight,
                    IsPassable = !isLocked,
                });
            }
            return trianglesData;
        }
    
    }
}
