using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.Tests
{
    public class FlattenedHexTest
    {
        private enum TrianglePositionId : byte { Undefined, InsideHex, OutsideHex}

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
            var trisCount = TriangularMath.GetTrianglesCountInHex(hexRadius+1);
            var trisDict = new Dictionary<IntTriangularPos, TrianglePositionId>(trisCount);

            var triangleHeight = HEX_EDGE / hexRadius * NavigationConstants.SQRT_OF_THREE_HALVED;
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, HEX_EDGE, triangleHeight);

            // make an outside zone
            foreach (var pos in new HexTrianglesEnumerator(hexPos, hexRadius+1))
            {
                trisDict.Add(pos, TrianglePositionId.OutsideHex);
            }

            // hex triangles zone will be inside
            foreach (var pos in new HexTrianglesEnumerator(hexPos, hexRadius))
            {
                trisDict[pos] = TrianglePositionId.InsideHex;
            }

            var flattenedHexCoordsConverter = new FlattenedHexCoordsConverter(
                hexPos.TriangularCenterPos,
                hexRadius,
                HEX_EDGE,
                triangleHeight,
                _rowIndicesData.AsReadOnly());

            foreach (var pos in new HexTrianglesEnumerator(hexPos, hexRadius + 1))
            {
                var canConvert = flattenedHexCoordsConverter.TryGetIndex(pos, out var index);
                var value = trisDict[pos];
                switch (value)
                {
                    case TrianglePositionId.InsideHex: Assert.IsTrue(canConvert, $"{pos} is inside hex, but cannot be defined by converter"); break;
                    case TrianglePositionId.OutsideHex: Assert.IsFalse(canConvert, $"{pos} is outside hex, but recognises as valid"); break;
                    default: Assert.Fail($"undefined position {pos}"); break;
                }
                if (!canConvert)
                    continue;
                var backpos = flattenedHexCoordsConverter.IndexToTriangular(index);
                Assert.AreEqual(pos, backpos, $"backpos doesnt match: {pos} -> {index} -> {backpos}");
            }

            for (var i = 0; i < TriangularMath.GetTrianglesCountInHex(hexRadius); i++)
            {
                Assert.IsTrue(flattenedHexCoordsConverter.TryGetTriangular(i, out var pos), $"cannot convert index {i}");

                Assert.IsTrue(trisDict.ContainsKey(pos), $"tris dict dont contain {i}:{pos}");
                Assert.IsTrue(trisDict[pos] == TrianglePositionId.InsideHex, $"{pos} is not inside hex");
                //TestContext.WriteLine($"{i} : {pos}");
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
