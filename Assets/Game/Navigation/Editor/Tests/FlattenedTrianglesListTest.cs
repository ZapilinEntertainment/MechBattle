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
        public void RowsListTest(int index, int rowValue)
        {
            Assert.AreEqual(rowValue, _rowIndicesData[index]);
        }


        [Order(2)]
        [TestCase(0,0,4)]
        [TestCase(0, 0, 5)]
        [TestCase(0, 0, 10)]
        [TestCase(0, 0, 32)]
        [TestCase(4, 4, 4)]
        [TestCase(4, 4, 32)]
        public void CoordinatesTest(int hexCoordX, int hexCoordY, int radius)
        {
            var hexEdgeLength = 100f;
            var hexPos = new NavigationHexPosition(new(hexCoordX, hexCoordY), hexEdgeLength, radius );         
            var readOnlyArray = _rowIndicesData.AsReadOnly();

            Span<TrianglesToIndexFlattenedConverter> converters = stackalloc TrianglesToIndexFlattenedConverter[6];
            var innerTriangles = NavigationMapHelper.GetOneTriangleHexPositions();
            converters[0] = new TrianglesToIndexFlattenedConverter(innerTriangles[0], radius, readOnlyArray);
            converters[1] = new TrianglesToIndexFlattenedConverter(innerTriangles[1] + new int3(-radius+1, radius-1, 0), radius, readOnlyArray);
            converters[2] = new TrianglesToIndexFlattenedConverter(innerTriangles[2] + new int3(0, -radius+1, radius-1), radius, readOnlyArray);
            converters[3] = new TrianglesToIndexFlattenedConverter(innerTriangles[3], radius, readOnlyArray);
            converters[4] = new TrianglesToIndexFlattenedConverter(innerTriangles[4] + new int3(radius-1, -radius+1, 0), radius, readOnlyArray);
            converters[5] = new TrianglesToIndexFlattenedConverter(innerTriangles[5] + new int3(0, radius-1, -radius+1), radius, readOnlyArray);

            foreach (var pos in new HexTrianglesEnumerator(hexPos, radius))
            {
                var sector = TriangularMath.DefineSector(pos, hexEdgeLength, radius);
                var sectorIndex = (int)sector;

                var converter = converters[sectorIndex];
                var index = converter.TriangularToIndex(pos);
                var backPos = converter.IndexToTriangular(index);
                Assert.AreEqual(pos, backPos, $"{sector} triangle failed: {pos} -> {index} -> {backPos}, {converter.TriangularToV2(pos)}");
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
