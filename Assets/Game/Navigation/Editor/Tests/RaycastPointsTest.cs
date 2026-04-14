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
        [TestCase(4, 4, 1)]
        [TestCase(0, -1, 4)]
        [TestCase(-32,32 , 8)]
        public void HexTrianglePointsTest(int hexCoordX, int hexCoordY, int trianglesPerHexEdge)
        {
            const float HEX_EDGE_SIZE = 100f;
            var triangleHeight = HEX_EDGE_SIZE / trianglesPerHexEdge * NavigationConstants.SQRT_OF_THREE_HALVED;
            const int RAYCAST_TRIANGLES_PER_EDGE = 4;
            var allocator = Allocator.TempJob;

            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, HEX_EDGE_SIZE, triangleHeight);
            var hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(trianglesPerHexEdge);
            var raycastCommandsCount = hexTrianglesCount * RAYCAST_TRIANGLES_PER_EDGE * RAYCAST_TRIANGLES_PER_EDGE;
            using var raycastCommands = new NativeArray<RaycastCommand>(raycastCommandsCount, allocator);

            var raycastsPerTriangle = RAYCAST_TRIANGLES_PER_EDGE * RAYCAST_TRIANGLES_PER_EDGE;
            using var raycastPointsArray = new NativeArray<SubdivideTriangleIntoSmallerOnesCommand.SmallTriangleData>(raycastsPerTriangle, allocator, NativeArrayOptions.UninitializedMemory);

            var positionsJob = new PrepareHexRaycastCommandsJob()
            {
                CastingHeight = NavigationConstants.CASTING_HEIGHT,
                CastingRayLength = NavigationConstants.CASTING_RAY_LENGTH,
                RaycastCommands = raycastCommands,
                QueryParameters = NavigationConstants.GetWalkableCastQueryParameters(),
                RaycastPoints = raycastPointsArray,
                RaycastTrianglesPerEdge = RAYCAST_TRIANGLES_PER_EDGE,
                TriangleHeight = triangleHeight,
                HexPos = hexPos,
                TrianglesPerEdge = trianglesPerHexEdge
            };
            var handle = positionsJob.ScheduleByRef();
            handle.Complete();


            var raycastCounts = new Dictionary<IntTriangularPos, int>(hexTrianglesCount);

            // CHECK IF ALL TRIANGLES HAVE CORRECT NUMBER OF RAYCASTS

            var uniqueTris = new HashSet<IntTriangularPos>();
            foreach (var command in positionsJob.RaycastCommands)
            {
                var trianglePos = TriangularMath.WorldToTrianglePos(command.from, triangleHeight);
                uniqueTris.Add(trianglePos);
                raycastCounts.TryGetValue(trianglePos, out var val);
                raycastCounts[trianglePos] = val + 1;
            }


            //foreach (var triPos in uniqueTris) TestContext.WriteLine(triPos);


            //TestContext.WriteLine($"{uniqueTris.Count} : {positionsArray.Length}");

            //foreach (var tripos in positionsArray)
            //{
            //    raycastCounts.TryGetValue(tripos, out var count);
            //    TestContext.WriteLine($"{tripos} : {count} : {uniqueTris.Contains(tripos)}");
            //    Assert.AreEqual(raycastsPerTriangle, count, $"{tripos} raycast count not match");
            //}

        }
    
    }
}
