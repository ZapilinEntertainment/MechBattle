using NUnit.Framework;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Editor.Tests
{
    public class TriangularDirectionDefinitionTest
    {
        [TestCase(10f)]
        public void NeighbourDefinitionTest(float triangleEdgeSize)
        {
            var triangleHeight = TriangularMath.GetTriangleHeight(triangleEdgeSize);

            TestContext.WriteLine("peaks:");
            var zeroPeakTripos = new IntTriangularPos(0, -1, 0);
            var peakOffsets = new PeakNeighbourOffsets();            
            var i = 0;
            foreach (var neighbourPos in new TriangleNeighboursEnumerator<PeakNeighbourOffsets>(zeroPeakTripos, peakOffsets))
            {
                var peakNeighbour = (PeakNeighbour)i;
                TestContext.WriteLine($"{peakNeighbour}: {neighbourPos}");
                Assert.AreEqual(TriangularMath.GetPeakNeighbour(zeroPeakTripos, i), neighbourPos, $"peak neighbour {peakNeighbour} doesn't match");
                Assert.AreEqual(peakNeighbour, TriangularMath.DefinePeakNeighbour(zeroPeakTripos, neighbourPos), $"peak neighbour {peakNeighbour} at {neighbourPos} defined incorrectly");
                i++;
            }


            TestContext.WriteLine("valleys:");
            var zeroValleyTripos = new IntTriangularPos(0, 1, 0);
            var valleyOffsets = new ValleyNeighbourOffsets();            
            i = 0;
            foreach (var neighbourPos in new TriangleNeighboursEnumerator<ValleyNeighbourOffsets>(zeroPeakTripos, valleyOffsets))
            {
                var valleyNeighbour = (ValleyNeighbour)i;
                TestContext.WriteLine($"{valleyNeighbour}: {neighbourPos}");
                Assert.AreEqual(TriangularMath.GetValleyNeighbour(zeroPeakTripos, i), neighbourPos, $"valley neighbour {valleyNeighbour} doesn't match");
                Assert.AreEqual(valleyNeighbour, TriangularMath.DefineValleyNeighbour(zeroPeakTripos, neighbourPos), $"peak neighbour {valleyNeighbour} at {neighbourPos} defined incorrectly");
                i++;
            }
        }
    
    }
}
