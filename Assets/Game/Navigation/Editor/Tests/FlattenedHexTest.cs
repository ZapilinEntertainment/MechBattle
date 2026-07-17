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

            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, hexRadius))
            {
                TestContext.WriteLine(pos);
            }

                // make an outside zone
                foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, hexRadius+1))
            {
                trisDict.Add(pos, TrianglePositionId.OutsideHex);
            }

            // hex triangles zone will be inside
            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, hexRadius))
            {
                trisDict[pos] = TrianglePositionId.InsideHex;
                TestContext.WriteLine(pos);
            }

            var flattenedHexCoordsConverter = new FlattenedHexCoordsConverter(
                hexPos.TriangularCenterPos,
                hexRadius,
                HEX_EDGE,
                triangleHeight,
                _rowIndicesData.AsReadOnly());

            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, hexRadius + 1))
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
                var backIndex = flattenedHexCoordsConverter.TriangularToIndex(pos);
                Assert.AreEqual(i, backIndex, $"back index not match: {i} -> {pos} -> {backIndex}");

                Assert.IsTrue(trisDict.ContainsKey(pos), $"tris dict dont contain {i}:{pos}");
                Assert.IsTrue(trisDict[pos] == TrianglePositionId.InsideHex, $"{pos} is not inside hex");
                //TestContext.WriteLine($"{i} : {pos}");
            }
        }

        [TestCase(0,1,0, 0,0, 2)]
        [TestCase(-1, 0, 0, 0, 0, 2)]
        public void ValueConversionTest(int x, int y, int z, int hexCoordX, int hexCoordY, int hexRadius)
        {
            var pos = new IntTriangularPos(x,y,z);

            var triangleHeight = HEX_EDGE / hexRadius * NavigationConstants.SQRT_OF_THREE_HALVED;
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, HEX_EDGE, triangleHeight);
            var hexConverter = new FlattenedHexCoordsConverter(hexPos.TriangularCenterPos, hexRadius, HEX_EDGE, triangleHeight, _rowIndicesData.AsReadOnly());

            var index = hexConverter.TriangularToIndex(pos);
            var backPos = hexConverter.IndexToTriangular(index);
            var backIndex = hexConverter.TriangularToIndex(backPos);

            TestContext.WriteLine($"{pos} -> {index} -> {backPos} -> {backIndex}");

            Assert.AreEqual(pos, backPos, "positions aren't same");
            Assert.AreEqual(index, backIndex, "index aren't same");
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
