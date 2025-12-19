using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class CoordinatesTest
    {
        private const float TRIANLGE_TEST_EDGE = 1f;
        private const float TOLERANCE = 0.001f;

        [Test]
        public void TestCoordinateConversionFixed()
        {
            var cartesianPos = new float3(0f,0f, 0.3f);
            var triangular = TriangularMath.CartesianToTriangular(cartesianPos, TRIANLGE_TEST_EDGE);
            Debug.Log(triangular);
            var cartesianBack = TriangularMath.TriangularToCartesian(triangular, TRIANLGE_TEST_EDGE);
            Assert.AreEqual(expected: cartesianPos.x, actual: cartesianBack.x, TOLERANCE);
            Assert.AreEqual(expected: cartesianPos.z, actual: cartesianBack.z, TOLERANCE);
        }


        [Test]
        [Repeat(256)]
        public void TestCoordinateConversion()
        {
            const float RADIUS = 1000f;

            var random = RADIUS * UnityEngine.Random.insideUnitCircle;
            var cartesian = new float3(random.x, 0f, random.y);
            var triangular = TriangularMath.CartesianToTriangular(cartesian, TRIANLGE_TEST_EDGE);
            var cartesianBack = TriangularMath.TriangularToCartesian(triangular, TRIANLGE_TEST_EDGE);
            Assert.AreEqual(expected: cartesian.x, actual: cartesianBack.x, TOLERANCE);
            Assert.AreEqual(expected: cartesian.z, actual: cartesianBack.z, TOLERANCE);
        }

        
    }
}
