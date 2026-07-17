using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Editor.Tests
{
    public class TrianglesRadiusTest
    {
        private const float TRIANGLE_EDGE = 10f;

        [TestCase(50f, 5)]
        public void TestTrianglesRadiusJob(float radius, int selectPositions)
        {
            var triangleHeight = TriangularMath.GetTriangleHeight(TRIANGLE_EDGE);
            var centerPos = new float3(-25f, 0f, 540f);

            using var resultsList = new NativeList<IntTriangularPos>(Allocator.TempJob);
            var job = new GetTrianglesInRadiusJob()
            {
                RadiusInUnits = radius,
                TriangleHeight = triangleHeight,
                WorldPos = centerPos,
                ResultList = resultsList
            };
            job.RunByRef();

            var radiusSq = radius* radius;
            foreach (var tripos in resultsList)
            {
                var pointPos = TriangularMath.TriangularToWorld(tripos, triangleHeight);
                Assert.IsTrue(math.distancesq(pointPos, centerPos) < radiusSq, $"triangle {tripos} is out of range: {math.distance(pointPos, centerPos)}/{radius}");
            }
        }
    
    }
}
