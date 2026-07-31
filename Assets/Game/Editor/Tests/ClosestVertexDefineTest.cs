using UnityEngine;
using NUnit.Framework;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Editor.Tests
{
    public class ClosestVertexDefineTest
    {
        private const float TRIANGLE_EDGE = 10f;

        [TestCase(43f, 0f, 20f)]
        public void Test(float worldX, float worldY , float worldZ)
        {
            var worldPos = new float3(worldX, worldY, worldZ);
            var triangleHeight = TriangularMath.GetTriangleHeight(TRIANGLE_EDGE);
            var cellTripos = TriangularMath.WorldToTrianglePos(worldPos, triangleHeight);

            var virtualTripos = GetClosestVertexTriposCommand.Execute(worldPos, triangleHeight, cellTripos);

            var dist = TriangularMath.CalculateTriangularDistance(cellTripos.ToFloat3(), virtualTripos.ToFloat3());
            TestContext.WriteLine($"{cellTripos} -> {virtualTripos}: {dist}");
            Assert.IsTrue(dist <= 0.5f);
        }
    
    }
}
