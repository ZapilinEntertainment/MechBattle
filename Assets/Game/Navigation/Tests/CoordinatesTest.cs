using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class CoordinatesTest
    {
        private const float TRIANGLE_EDGE_SIZE = 1f;
        private const float TOLERANCE = 0.001f;

        [TestCase(0f,0f,0.3f)]
        [TestCase(150f, 0f, 86.6f)]
        public void CartesianTriangularCartesian(float x, float y, float z)
        {
            var cartesianPos = new float3(x,y,z);
            var triangular = TriangularMath.CartesianToTriangular(cartesianPos, TRIANGLE_EDGE_SIZE);
            Debug.Log(triangular);
            var cartesianBack = TriangularMath.TriangularToCartesian(triangular, TRIANGLE_EDGE_SIZE);
            Assert.AreEqual(expected: cartesianPos.x, actual: cartesianBack.x, TOLERANCE);
            Assert.AreEqual(expected: cartesianPos.z, actual: cartesianBack.z, TOLERANCE);
        }

        [TestCase(0,1,0)]
        [TestCase(0, 2, 0)]
        [TestCase(1, 0, 0)]
        [TestCase(0, 0, 1)]
        [TestCase(-5,1,5)]
        [TestCase(-3,1,3)]
        [TestCase(-4, 3, 2)]
        public void TriangularCartesianTriangular(int x, int y, int z)
        {
            var triangle = TriangularMath.Standartize(new IntTriangularPos(x,y,z));
            var cartesian = TriangularMath.TriangularToCartesian(triangle, TRIANGLE_EDGE_SIZE);
            var triangleBack = TriangularMath.Standartize(TriangularMath.CartesianToTrianglePos(cartesian, TRIANGLE_EDGE_SIZE));

            Debug.Log($"{triangle} -> {cartesian} -> {triangleBack}");

            Assert.AreEqual(expected: triangle.DownLeft, actual: triangleBack.DownLeft);
            Assert.AreEqual(expected: triangle.Up, actual: triangleBack.Up);
            Assert.AreEqual(expected: triangle.DownRight, actual: triangleBack.DownRight);
        }


        [Test]
        [Repeat(256)]
        public void TestCoordinateConversion()
        {
            const float RADIUS = 1000f;

            var random = RADIUS * UnityEngine.Random.insideUnitCircle;
            var cartesian = new float3(random.x, 0f, random.y);
            var triangular = TriangularMath.CartesianToTriangular(cartesian, TRIANGLE_EDGE_SIZE);
            var cartesianBack = TriangularMath.TriangularToCartesian(triangular, TRIANGLE_EDGE_SIZE);
            Assert.AreEqual(expected: cartesian.x, actual: cartesianBack.x, TOLERANCE);
            Assert.AreEqual(expected: cartesian.z, actual: cartesianBack.z, TOLERANCE);
        }

        
    }
}
