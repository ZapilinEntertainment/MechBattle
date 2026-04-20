using NUnit.Framework;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class DefaultSectorFlowTest
    {
        [TestCase(0, 0, 1, 50)]
        [TestCase(0, 0, 3, 50)]
        [TestCase(0, 0, 8, 50)]
        [TestCase(0, 0, 16, 50)]
        [TestCase(4, 4, 4, 50f)]
        [TestCase(4, 4, 4, 25f)]
        [TestCase(-6, -18, 8, 50f)]
        public void Check(int hexCenterX, int hexCenterY, int hexRadius, float hexEdgeLength)
        {
            var hexPos = new NavigationHexPosition(new int2(hexCenterX, hexCenterY), hexEdgeLength, hexRadius);
            var center = hexPos.TriangularCenterPos;
            var triangleHeight = hexEdgeLength / hexRadius * NavigationConstants.SQRT_OF_THREE_HALVED;

            for (var e = 0; e < 6; e++)
            {
                var exitEdge = (HexEdge)e;

                foreach (var pos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, hexRadius))
                {
                    var sector = TriangularMath.DefineSector(pos, hexEdgeLength, hexRadius, triangleHeight);
                    var defaultDirection = sector.GetDefaultFlowDirection(exitEdge, pos.IsPeak);
                    var nextPos = TriangularMath.GetNeighbourByDirection(pos, defaultDirection);
                    var nextPosHex = TriangularMath.TriangularToHex(nextPos, triangleHeight, hexEdgeLength);

                    if ((int)sector == (int)exitEdge)
                    {
                        var exitHex = exitEdge.ToHexOffsetVector() + hexPos.HexCoordinate;
                        
                        if (!math.all(nextPosHex == hexPos.HexCoordinate))
                        {
                            Assert.IsTrue(
                                math.all(nextPosHex == exitHex),
                                $"sector: {sector} exit edge: {exitEdge}  {pos} -> {nextPos} is out of hex: {nextPosHex}");
                        }
                        else
                        {
                            var nextPosSector = TriangularMath.DefineSector(nextPos, hexEdgeLength, hexRadius, triangleHeight);
                            Assert.AreEqual(sector, nextPosSector, $"sector: {sector} exit edge: {exitEdge}  {pos} -> {nextPos} is out of sector {nextPosSector} / {sector}");
                        }
                    }
                    else
                    {
                        Assert.AreEqual(hexPos.HexCoordinate, nextPosHex, $"sector: {sector} exit edge: {exitEdge}  {pos} -> {nextPos} is out of hex: {nextPosHex}");
                    }
                }
            }
            
        }
    
    }
}
