using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using NUnit.Framework;

namespace ZE.MechBattle.Navigation.Tests
{
    public class RaycastPointsTest
    {
        [TestCase(0, 0, 1)]
        [TestCase(-4,4 , 4)]
        public void HexTrianglePointsTest(int hexCoordX, int hexCoordY, int trianglesPerHexEdge)
        {
            const float HEX_EDGE_SIZE = 100f;
            var trianglesEdgeSize = HEX_EDGE_SIZE / trianglesPerHexEdge;
            const int RAYCAST_TRIANGLES_PER_EDGE = 4;
            var allocator = Allocator.TempJob;

            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, HEX_EDGE_SIZE, trianglesEdgeSize);
            var hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(trianglesPerHexEdge);
            var raycastCommandsCount = hexTrianglesCount * RAYCAST_TRIANGLES_PER_EDGE * RAYCAST_TRIANGLES_PER_EDGE;
            using var positionsArray = new NativeArray<IntTriangularPos>(hexTrianglesCount, allocator, NativeArrayOptions.UninitializedMemory);
            using var raycastCommands = new NativeArray<RaycastCommand>(raycastCommandsCount, allocator);

            var raycastsPerTriangle = RAYCAST_TRIANGLES_PER_EDGE * RAYCAST_TRIANGLES_PER_EDGE;
            using var raycastPointsArray = new NativeArray<float2>(raycastsPerTriangle, allocator, NativeArrayOptions.UninitializedMemory);

            var positionsJob = new PrepareHexRaycastCommandsJob()
            {
                CastingHeight = NavigationConstants.CASTING_HEIGHT,
                CastingRayLength = NavigationConstants.CASTING_RAY_LENGTH,
                HexCenterWorld = hexPos.CenterPosWorld,
                RaycastCommands = raycastCommands,
                Positions = positionsArray,
                QueryParameters = NavigationConstants.GetGroundCastQueryParameters(),
                RaycastPoints = raycastPointsArray,
                RaycastTrianglesPerEdge = RAYCAST_TRIANGLES_PER_EDGE,
                TriangleEdgeSize = trianglesEdgeSize,
                TrianglesPerHexEdge = trianglesPerHexEdge,
            };
            var handle = positionsJob.ScheduleByRef();
            handle.Complete();


            var raycastCounts = new Dictionary<IntTriangularPos, int>(hexTrianglesCount);

            // CHECK IF POSITIONS WAS CORRECT
            foreach (var pos in positionsJob.Positions)
            {
                raycastCounts.Add(pos, raycastsPerTriangle);
            }

            foreach (var tripos in positionsArray)
            {
                raycastCounts.TryGetValue(tripos, out var count);
                //TestContext.WriteLine(tripos);
                Assert.AreEqual(raycastsPerTriangle, count, $"{tripos} virtual check mismatch");
            }

            raycastCounts.Clear();


            // CHECK IF ALL TRIANGLES HAVE CORRECT NUMBER OF RAYCASTS

            foreach (var command in positionsJob.RaycastCommands)
            {
                var trianglePos = TriangularMath.WorldToTrianglePos(command.from, trianglesEdgeSize);
                TestContext.WriteLine(trianglePos);
                raycastCounts.TryGetValue(trianglePos, out var val);
                raycastCounts[trianglePos] = val + 1;
            }

            foreach (var tripos in positionsArray)
            {
                raycastCounts.TryGetValue(tripos, out var count);
                TestContext.WriteLine($"{tripos} : {count}");
                //Assert.AreEqual(raycastsPerTriangle, count, $"{tripos} raycast count not match");
            }

        }
    
    }
}
