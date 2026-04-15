using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.Tests
{
    public class FlattenedHexTest
    {
        private const float HEX_EDGE = 100f;
        private NativeArray<byte> _rowIndicesData;


        [TestCase(0,0,1)]
        [TestCase(0, 0, 2)]
        [TestCase(0, 0, 4)]
        [TestCase(0, 0, 32)]
        [TestCase(4, 4, 1)]
        [TestCase(4, 4, 4)]
        public void CoordsConverterTest(int hexCoordX, int hexCoordY, int hexRadius)
        {
            var trisCount = TriangularMath.GetTrianglesCountInHex(hexRadius);
            var trisDict = new Dictionary<IntTriangularPos, bool>(trisCount);

            var triangleHeight = HEX_EDGE / hexRadius * NavigationConstants.SQRT_OF_THREE_HALVED;
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, HEX_EDGE, triangleHeight);
            foreach (var pos in new HexTrianglesEnumerator(hexPos, hexRadius))
            {
                trisDict.Add(pos, false);
            }

            var flattenedHexCoordsConverter = new HexFlattenedCoordConverter(
                hexPos.TriangularCenterPos,
                hexRadius,
                HEX_EDGE,
                triangleHeight,
                _rowIndicesData.AsReadOnly());

            for (var i = 0; i < trisCount; i++)
            {
                Assert.IsTrue(flattenedHexCoordsConverter.TryGetTriangular(i, out var pos), $"cannot convert {i}");
                Assert.IsTrue(trisDict.ContainsKey(pos), $"tris dict dont contain {i}:{pos}");
                trisDict[pos] = true;
                //TestContext.WriteLine($"{i} : {pos}");
            }

            foreach (var kvp in trisDict)
            {
                Assert.IsTrue(kvp.Value, $"{kvp.Key} was not recognised");
            }
        }

        [OneTimeSetUp]
        public void Setup()
        {
            _rowIndicesData = TrianglesToIndexFlattenedConverter.FulfilRowIndices(Allocator.TempJob, 32);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _rowIndicesData.Dispose();
        }
    }
}
