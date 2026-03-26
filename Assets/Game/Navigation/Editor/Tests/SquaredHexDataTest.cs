using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using NUnit.Framework;

namespace ZE.MechBattle.Navigation.Tests
{
    public class SquaredHexDataTest
    {
        [TestCase(0,0,0)]
        [TestCase(100,0,-100)]
        [TestCase(-98, 0, 98)]
        [TestCase(-128, 128, 0)]
        [TestCase(0, 256, -256)]
        [TestCase(164, -164, 0)]
        [TestCase(0, -1024, 1024)]
        public void OneTriangleHexSquaring(int hexCenterX, int hexCenterY, int hexCenterZ)
        {
            var center = new IntTriangularPos(hexCenterX, hexCenterY, hexCenterZ);
            using var squaredArray = new SquaredHexTrianglesList<int>(center, 1, Allocator.Temp);
            var converter = squaredArray.CoordsConverter;

            var tris = new IntTriangularPos[8];
            tris[1] = center + new int3(1,0,0);
            tris[2] = center + new int3(0,0,-1);
            tris[3] = center + new int3(0,1,0);
            tris[0] = TriangularMath.GetValleyNeighbour(tris[1], ValleyNeighbour.EdgeDownLeft);

            tris[4] = center + new int3(0,-1,0);
            tris[5] = center + new int3(0,0,1);
            tris[6] = center + new int3(-1, 0,0);
            tris[7] = TriangularMath.GetPeakNeighbour(tris[6], PeakNeighbour.EdgeUpRight);

            for (var i = 0; i < tris.Length; i++)
            {
                Debug.Log($"{i} : {tris[i]}");
                squaredArray.Set(tris[i], i);
            }

            //for (var i = 0; i < squaredArray.Length; i++) Debug.Log($"{i} : {tris[i]} / {converter.IndexToTriangular(i)} : {squaredArray[i]} \n");

            for (var i = 0; i < squaredArray.Length; i++)
            {
                Assert.AreEqual(true, squaredArray.TryGet(tris[i], out var result), $"{tris[i]} not exist");
                Assert.AreEqual(i, result, $"{tris[i]} is not {i} but {result}");
            }

            var v1 = tris[0];
            Assert.AreEqual(true, squaredArray.TryGet(v1, out var f1), $"{v1} out of range");
            Assert.AreEqual(0, f1, $"{v1} has wrong value");

            var v2 = tris[7];
            Assert.AreEqual(true, squaredArray.TryGet(v2, out var f2), $"{v2} out of range");
            Assert.AreEqual(7, f2, $"{v2} has wrong value");
        }

        [TestCase(1, 0,0,0)]
        [TestCase(2, 0, 0, 0)]
        [TestCase(4, 0,0,0)]
        [TestCase(32, 0, -96, 96)]
        public void VariableTriangleHexSquaring(int radius, int hexCenterX, int hexCenterY, int hexCenterZ)
        {
            var center = new IntTriangularPos(hexCenterX, hexCenterY, hexCenterZ);
            using var squaredArray = new SquaredHexTrianglesList<int>(center, radius, Allocator.TempJob);
            var converter = squaredArray.CoordsConverter;

            var hexTrisCount = TriangularMath.GetTrianglesCountInHex(radius);
            var excessTrisCount = 2 * radius * radius; // excess trison bottom left and top right corners
            var trisCount = hexTrisCount + excessTrisCount;
            //Debug.Log($"radius: {radius}, tris inside hex: {hexTrisCount}, excess tris: {excessTrisCount}, total: {trisCount}");
            Assert.AreEqual(trisCount, squaredArray.Length, "squared array length not fit");
            
            var tris = new IntTriangularPos[trisCount];
            var startPeakPos = converter.BottomLeftPeakTrianglePos;
            var startValleyPos = converter.BottomLeftValleyTrianglePos;

            tris[0] = startPeakPos;
            tris[1] = startValleyPos;
            var index = 2;

            squaredArray.Set(startPeakPos, 0);
            squaredArray.Set(startValleyPos, 1);
            TestContext.WriteLine($"({0},{0}) : {startPeakPos} : {0}");
            TestContext.WriteLine($"({0},{1}) : {startValleyPos} : {1}");

            for (var x = 0; x < converter.ArrayWidth; x++)
            {
                for (var y = 0; y < converter.ArrayHeight; y++)
                {
                    if (x == 0 && (y == 0 || y == 1))
                        continue;

                    var pos = y % 2 == 1 
                        ? TriangularMath.GetPeakNeighbour(tris[index- 1], PeakNeighbour.EdgeUpRight) 
                        : TriangularMath.GetValleyNeighbour(tris[index-1], ValleyNeighbour.EdgeUp);

                    squaredArray.Set(pos, index);
                    TestContext.WriteLine($"({x},{y}) : {pos} : {index}");                    
                    tris[index++] = pos;                    
                }

                startPeakPos = TriangularMath.GetPeakNeighbour(startPeakPos, PeakNeighbour.VertexRight);
                startValleyPos = TriangularMath.GetValleyNeighbour(startValleyPos, ValleyNeighbour.VertexRight);                
            }


            for (var i = 0; i < squaredArray.Length; i++)
            {
                Assert.AreEqual(true, squaredArray.TryGet(tris[i], out var result), $"{tris[i]} not exist");
                Assert.AreEqual(i, result, $"{tris[i]} is not {i} but {result}");
            }
        }
    }
}
