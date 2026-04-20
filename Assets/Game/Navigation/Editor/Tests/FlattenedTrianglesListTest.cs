using UnityEngine;
using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using System;

namespace ZE.MechBattle.Navigation.Tests
{
    public class FlattenedTrianglesListTest
    {
        private NativeArray<byte> _rowIndicesData;

        [Order(1)]
        [TestCase(0,0)]
        [TestCase(5, 2)]
        [TestCase(6, 3)]
        [TestCase(12, 4)]
        public void RowsListConstantsTest(int index, int rowValue)
        {
            Assert.AreEqual(rowValue, _rowIndicesData[index]);
        }

        [TestCase(32)]
        public void RowsListCalculatedTest(int radius)
        {
            using var indices = TrianglesToIndexFlattenedConverter.FulfilRowIndices(Allocator.TempJob, radius);
            Assert.AreEqual(radius * (radius + 1) / 2, indices.Length, "incorrect count");
            var readIndex = 0;
            for (var i = 0; i < radius; i++)
            {
                var count = i + 1;
                for (var j = 0; j < count; j++)
                {
                    Assert.AreEqual(i, indices[readIndex], $"failed at index {i}, read index: {readIndex}, value: {indices[readIndex]}");
                    readIndex++;
                }
            }
        }


        [Order(2)]
        [TestCase(0,0,4)]
        [TestCase(0, 0, 5)]
        [TestCase(0, 0, 10)]
        [TestCase(0, 0, 32)]
        [TestCase(-1, 1, 2)]
        [TestCase(4, 4, 4)]
        [TestCase(4, 4, 32)]
        public void CoordinatesCalculationTest(int hexCoordX, int hexCoordY, int radius)
        {
            const float HEX_EDGE = 100f;
            var hexPos = new NavigationHexPosition(new int2(hexCoordX, hexCoordY), HEX_EDGE, radius );         
            var readOnlyArray = _rowIndicesData.AsReadOnly();

            Span<TrianglesToIndexFlattenedConverter> converters = stackalloc TrianglesToIndexFlattenedConverter[6];
            var innerTriangles = NavigationMapHelper.GetSixInnerRingTriangles();
            if (math.lengthsq(hexPos.TriangularCenterPos.ToFloat3()) != 0)
            {
                var center = hexPos.TriangularCenterPos;
                TestContext.WriteLine("hex center: " + center.ToString());
                for (var i = 0; i < 6; i++)
                {
                    innerTriangles[i] += center;
                }
            }

            //Debug.Log(innerTriangles[0]);

            converters[0] = new TrianglesToIndexFlattenedConverter(innerTriangles[0], radius, readOnlyArray);
            converters[1] = new TrianglesToIndexFlattenedConverter(innerTriangles[1] + new int3(-radius+1, radius-1, 0), radius, readOnlyArray);
            converters[2] = new TrianglesToIndexFlattenedConverter(innerTriangles[2] + new int3(0, -radius+1, radius-1), radius, readOnlyArray);
            converters[3] = new TrianglesToIndexFlattenedConverter(innerTriangles[3], radius, readOnlyArray);
            converters[4] = new TrianglesToIndexFlattenedConverter(innerTriangles[4] + new int3(radius-1, -radius+1, 0), radius, readOnlyArray);
            converters[5] = new TrianglesToIndexFlattenedConverter(innerTriangles[5] + new int3(0, radius-1, -radius+1), radius, readOnlyArray);

            var triangleHeight = HEX_EDGE / radius * NavigationConstants.SQRT_OF_THREE_HALVED;
            foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, radius))
            {
                var sector = TriangularMath.DefineSector(pos, HEX_EDGE, radius, triangleHeight);
                var sectorIndex = (int)sector;
                Assert.IsTrue(sectorIndex >=0 & sectorIndex < 6, "invalid sector index");
                var converter = converters[sectorIndex];
                Assert.IsTrue(converter.TryGetIndex(pos, out var index), $"cannot recognise {pos} as convertible ({sector})");
                Assert.IsTrue(index >=0, $"negative index: {pos} -> {index}");
                Assert.IsTrue(converter.TryGetTriangular(index, out var backPos), $"cannot revert {pos} back, ({sector}) | ({index})");
                Assert.AreEqual(pos, backPos, $"{sector} triangle failed: {pos} -> {index} -> {backPos}, {converter.TriangularToV2(pos)}");

                var directIndex = converter.TriangularToIndex(pos);
                var directBackPos = converter.IndexToTriangular(directIndex);
                Assert.AreEqual(pos, directBackPos, $"direct conversions not working: {sector} {pos} -> {directIndex}");
                Assert.IsTrue(index >= 0, $"negative index: {pos} -> {directIndex}");
            }
        }

        [TestCase(0, 0, 2)]
        [TestCase(0, 0, 4)]
        [TestCase(4, 4, 2)]
        public void PerSectorCoordsCheck(int hexCoordX, int hexCoordY, int radius)
        {
            const float HEX_EDGE = 100f;
            var hexPos = new NavigationHexPosition(new int2(hexCoordX, hexCoordY), HEX_EDGE, radius);
            var readOnlyArray = _rowIndicesData.AsReadOnly();

            Span<TrianglesToIndexFlattenedConverter> converters = stackalloc TrianglesToIndexFlattenedConverter[6];
            var innerTriangles = NavigationMapHelper.GetSixInnerRingTriangles();
            if (math.lengthsq(hexPos.TriangularCenterPos.ToFloat3()) != 0)
            {
                var center = hexPos.TriangularCenterPos;
                TestContext.WriteLine("hex center: " + center.ToString());
                for (var i = 0; i < 6; i++)
                {
                    innerTriangles[i] += center;
                }
            }

            //Debug.Log(innerTriangles[0]);

            converters[0] = new TrianglesToIndexFlattenedConverter(innerTriangles[0], radius, readOnlyArray);
            converters[1] = new TrianglesToIndexFlattenedConverter(innerTriangles[1] + new int3(-radius + 1, radius - 1, 0), radius, readOnlyArray);
            converters[2] = new TrianglesToIndexFlattenedConverter(innerTriangles[2] + new int3(0, -radius + 1, radius - 1), radius, readOnlyArray);
            converters[3] = new TrianglesToIndexFlattenedConverter(innerTriangles[3], radius, readOnlyArray);
            converters[4] = new TrianglesToIndexFlattenedConverter(innerTriangles[4] + new int3(radius - 1, -radius + 1, 0), radius, readOnlyArray);
            converters[5] = new TrianglesToIndexFlattenedConverter(innerTriangles[5] + new int3(0, radius - 1, -radius + 1), radius, readOnlyArray);

            var triangleHeight = HEX_EDGE / radius * NavigationConstants.SQRT_OF_THREE_HALVED;
            var trianglesPerSector = radius * radius;
            for (var edge = 0; edge < 6; edge++)
            {
                TestContext.WriteLine((HexEdge)edge);
                var converter = converters[edge];
                for (var i = 0; i < trianglesPerSector; i++)
                {
                    Assert.IsTrue(converter.TryGetTriangular(i, out var pos), $"index defined failed: {i} -> {pos}");
                    Assert.IsTrue(converter.TryGetIndex(pos, out var backIndex), $"backIndex cannot be found: {i} -> {pos} -> {backIndex}");
                    Assert.AreEqual(i, backIndex, $"back index is not correct: {i} -> {pos} -> {backIndex}");

                    TestContext.WriteLine($"{i}: {pos}");
                }
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
