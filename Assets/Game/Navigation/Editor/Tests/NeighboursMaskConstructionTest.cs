using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class NeighboursMaskConstructionTest
    {
        private const int TRIANGLES_PER_HEX_EDGE = 7;

        [TestCase(0,6,-5, int.MaxValue)]
        [TestCase(-1, 5, -5, 0b_1011_1111_1111)]
        [TestCase(-5,5,-1, 0b_1111_1111_1011)]
        public void SimpleTest(int x, int y, int z, int mask)
        {
            var tripos = new IntTriangularPos(x,y,z);
            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, 10, unscannedSurfacesArePassable: true);
            using var map = new NavigationMap(mapSettings, Allocator.Temp);

            for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
            {
                if ((mask & (1<< i)) == 0 )
                {
                    var neighbourPos = TriangularMath.GetNeighbourByDirection(tripos, i);
                    LockCell(neighbourPos, map);
                }                    
            }

            var logic = new UpdateCellNeighboursMaskLogic<CellHeightData, INavigationMap>(tripos, map, float.MaxValue);
            var neighbourMask = logic.CalculateNeighboursMask();

            for (var i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
            {
                var partMask = (1 << i);
                Assert.AreEqual(mask & partMask, neighbourMask & partMask, $"masks not matched at {i}");
            }
        }


        [TestCase(0,0,  0,  7,  0b_0111110_1110)]
        public void EdgeTrisTest(int hexCoordX, int hexCoordY, int edgeIndex, int trianglesPerEdge, int mask)
        {
            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, 10, unscannedSurfacesArePassable: true);
            using var map = new NavigationMap(mapSettings, Allocator.Temp);

            var hexPos = new NavigationHexPosition(new int2(hexCoordX, hexCoordY), map);
            int i = 0;
            var edge = (HexEdge)edgeIndex;
            var enumerator = edge.GetEdgeEnumerable(trianglesPerEdge, hexPos);
            foreach (var tripos in enumerator)
            {
                if ((mask & (1 << i)) == 0) 
                    LockCell(tripos, map);
                i++;
            }

            enumerator.Reset();            
            foreach (var tripos in enumerator)
            {
                var logic = new UpdateCellNeighboursMaskLogic<CellHeightData, INavigationMap>(tripos, map, float.MaxValue);
                UpdateNeighboursMask(tripos, map, logic.CalculateNeighboursMask());
            }


            for (i = 0; i < NavigationConstants.TRIANGLE_DIRECTIONS_COUNT; i++)
            {

            }
        }

        private void LockCell(IntTriangularPos pos, IUpdatableMap map)
        {
            var passability = map.GetPassabilityData(pos);
            passability.IsPassable = false;
            map.UpdateCellPassability(pos, passability);
        }

        private void UpdateNeighboursMask(IntTriangularPos pos, IUpdatableMap map, int mask)
        {
            var passability = map.GetPassabilityData(pos);
            passability.NeighboursMask = mask;
            map.UpdateCellPassability(pos, passability);
        }
    }
}
