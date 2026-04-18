using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using ZE.MechBattle.Navigation.DataStoring;

namespace ZE.MechBattle.Navigation.Tests
{
    public class DataEncodingTest
    {
        [TestCase(0,0)]
        [TestCase(0, 3)]
        [TestCase(1,3)]
        [TestCase(-1,8)]
        [TestCase(256,1)]
        [TestCase(-32768, 4)]
        public void IntEncodingTest(int value, int offset)
        {
            using var array = new NativeArray<byte>(4 + offset, Allocator.Temp);
            var slice = array.Slice(0, array.Length);
            ByteCoder.WriteIntToBufferLittleEndian(slice, value, offset);
            var result = ByteCoder.ReadIntFromBufferLittleEndian(slice, offset);
            Assert.AreEqual(value, result);
        }

        [TestCase(0, 0,0,0)]
        [TestCase(0, 0, 0, 4)]
        [TestCase(0, 1, 0, 2)]
        [TestCase(-1, 2, 1, 3)]
        [TestCase(76, 2, -74, 5)]
        public void IntTriangularPosEncodingTest(int x, int y, int z, int offset)
        {
            using var array = new NativeArray<byte>(IntTriangularPos.SERIALIZATION_LENGTH + offset, Allocator.Temp);
            var slice = array.Slice(0, array.Length);
            var pos = new IntTriangularPos(x,y,z);

            ByteCoder.WriteInt3ToBufferLittleEndian(slice, pos.ToInt3(), offset);
            var result = ByteCoder.ReadInt3FromBufferLittleEndian(slice, offset);
            Assert.AreEqual(pos, new IntTriangularPos(result));
        }

        [TestCase(0,0,true)]
        [TestCase(0, 2, true)]
        [TestCase(-1, 0, false)]
        [TestCase(4,4,false)]
        public void VirtualFlowMapEncodingTest(int hexCoordX, int hexCoordY, bool defaultPassability)
        {
            using var map = new NavigationMap(new(100f, 8, MapSettings.GetDefaultMapBorders()), Allocator.Temp);
            var virtualMap = new VirtualFlowMap(map, HexEdgesAccessMap.FullAccessMap, true);
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, map.HexEdgeSize, map.TriangleHeight);

            var storedData = FlowMapDecodingHandler.Encode(map, virtualMap, hexPos);
            var decodedData = FlowMapDecodingHandler.Decode(storedData, hexPos, map);

            Assert.AreEqual(virtualMap.GetAccessMap(), decodedData.GetAccessMap());
            var pos = hexPos.InnerRingTopValleyTriangle;
            Assert.AreEqual(virtualMap.GetCombinedCellData(pos), decodedData.GetCombinedCellData(pos));
        }

        [Test]
        public void CombinedFlowMapCellEncodingTest()
        {
            Span<int> cells = stackalloc int[6];
            for (var i = 0; i < 6; i++)
            {
                cells[i] = new FlowMapCellData(i, (ushort)i).Value;
            }

            var triangleData = new CellPassabilityData(true, 25, 3);

            var originalCombinedCellData = new FlowMapCombinedCell(cells, triangleData);

            var pos = new IntTriangularPos(1, 2, 3);
            using var writeBytes = new NativeArray<byte>(IntTriangularPos.SERIALIZATION_LENGTH + FlowMapCombinedCell.SERIALIZATION_LENGTH, Allocator.Temp);
            var writeSlice = writeBytes.Slice(0, writeBytes.Length);
            FlowMapCombinedCell.Encode(writeSlice, originalCombinedCellData, 0);
            var decodedData = FlowMapCombinedCell.Decode(writeSlice, 0);

            using var checkBytes = new NativeArray<byte>(IntTriangularPos.SERIALIZATION_LENGTH + FlowMapCombinedCell.SERIALIZATION_LENGTH, Allocator.Temp);
            var checkSlice = checkBytes.Slice(0, checkBytes.Length);
            FlowMapCombinedCell.Encode(checkSlice, decodedData, 0);
            Assert.AreEqual(originalCombinedCellData, decodedData, ShowFailedByte(writeSlice, checkSlice));
        }

        [Test]
        public void CombinedFlowMapCellEncodingTest2()
        {
            Span<int> cells = stackalloc int[6];
            for (var i = 0; i < 6; i++)
            {
                cells[i] = new FlowMapCellData(i,(ushort)i).Value;
            }
            
            var triangleData = new CellPassabilityData(true, 25, 3);

            var originalCombinedCellData = new FlowMapCombinedCell(cells, triangleData);

            var pos = new IntTriangularPos(1,2,3);
            using var writeBytes = new NativeArray<byte>(IntTriangularPos.SERIALIZATION_LENGTH + FlowMapCombinedCell.SERIALIZATION_LENGTH, Allocator.Temp);
            var writeSlice = writeBytes.Slice(0, writeBytes.Length);
            NavDataEncodingLogic.Encode(writeSlice, pos, originalCombinedCellData);
            var decodedData = NavDataEncodingLogic.Decode(writeSlice);

            Assert.AreEqual(pos, decodedData.pos);

            using var checkBytes = new NativeArray<byte>(IntTriangularPos.SERIALIZATION_LENGTH + FlowMapCombinedCell.SERIALIZATION_LENGTH, Allocator.Temp);
            var checkSlice = checkBytes.Slice(0, checkBytes.Length);
            NavDataEncodingLogic.Encode(checkSlice, pos, decodedData.cellData);
            Assert.AreEqual(originalCombinedCellData, decodedData.cellData, ShowFailedByte(writeSlice, checkSlice));
        }

        private string ShowFailedByte(NativeSlice<byte> writeBytes, NativeSlice<byte> readBytes)
        {
            for (var i = 0; i < writeBytes.Length; i++)
            {
                if (writeBytes[i] != readBytes[i]) 
                { 
                    return $"failed at byte {i}";
                }
            }
            return "nothing found";
        }

        [TestCase(0, 0, true, 2)]
        [TestCase(-1, 3, false,4)]
        [TestCase(4, 4, true,8)]
        public void FullMapEncodingTest(int hexCoordX, int hexCoordY, bool defaultPassability, int radius)
        {
            using var map = new NavigationMap(new(100f, radius, MapSettings.GetDefaultMapBorders()), Allocator.Temp);
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, map.HexEdgeSize, map.TriangleHeight);

            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(map.TrianglesPerHexEdge);
            var flowData = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(trianglesInHex, Allocator.TempJob);
            var random = new Random();
            Span<int> values = stackalloc int[6];
            foreach (var tripos in new HexTrianglesEnumerator(hexPos, map.TrianglesPerHexEdge))
            {
                var rvalue = random.Next();
                if (rvalue < 0)
                    continue;
                var triangleData = new CellPassabilityData(rvalue % 2 == 0, (short)rvalue, 1);
                
                for (var i = 0; i < 6; i++)
                {
                    values[i] = new FlowMapCellData(rvalue % 12, 3).Value;
                }

                flowData.Add(tripos, new(values, triangleData));
            }

            using var flowMap = new HexFlowMap(flowData, HexEdgesAccessMap.FullAccessMap);

            var storedData = FlowMapDecodingHandler.Encode(map, flowMap, hexPos);
            using var decodedData = FlowMapDecodingHandler.Decode(storedData, hexPos, map) as HexFlowMap;

            Assert.AreEqual(flowMap.GetAccessMap(), decodedData.GetAccessMap());

            foreach (var triposKvp in flowData)
            {
                Assert.AreEqual(triposKvp.Value, decodedData.GetCombinedCellData(triposKvp.Key));
            }
        }
    }
}
