using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class HexCoordsTest
    {
        [TestCase(0,0)]
        [TestCase(0,-5)]
        [TestCase(31, 4)]
        [TestCase(-1,0)]
        public void NeighbourCoordsTest(int x, int y)
        {
            const int OFFSET = 5;
            var hexCoord = new int2(x,y);
            var neighboursDetected = new HashSet<int2>();
            for (var i = -OFFSET; i < OFFSET; i++)
            {
                for (var j = -OFFSET; j < OFFSET; j++)
                {
                    var coord = new int2(i + hexCoord.x,j + hexCoord.y);
                    if (HexMath.AreNeighbours(hexCoord, coord))
                        neighboursDetected.Add(coord);
                }
            }

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                var correctNeighbourPos = hexCoord + edge.ToHexOffsetVector();
                Assert.IsTrue(neighboursDetected.Contains(correctNeighbourPos), $"{edge} neighbour missed");
            } 

            var countCorrect = neighboursDetected.Count == 6;
            if (!countCorrect)
            {
                foreach (var coord in neighboursDetected)
                {
                    TestContext.Write($"{coord} ");
                }
            }
            Assert.IsTrue(countCorrect, "excess neighbours found");
        }
    
    }
}
