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
            var hexPos = new NavigationHexPosition(new int2(hexCoordX, hexCoordY), hexEdgeLength, radius );         
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

            foreach (var pos in new HexTrianglesEnumerator(hexPos, radius))
            {
                var sector = TriangularMath.DefineSector(pos, hexEdgeLength, radius);
                var sectorIndex = (int)sector;
                Assert.IsTrue(sectorIndex >=0 & sectorIndex < 6, "invalid sector index");
                var converter = converters[sectorIndex];
                Assert.IsTrue(converter.TryGetIndex(pos, out var index), $"cannot recognise {pos} as convertible ({sector})");
                Assert.IsTrue(converter.TryGetTriangular(index, out var backPos), $"cannot revert {pos} back, ({sector}) | ({index})");
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
