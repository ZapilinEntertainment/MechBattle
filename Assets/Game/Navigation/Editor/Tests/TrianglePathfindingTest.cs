using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class TrianglePathfindingTest
    {
        [TestCase(1,21,-21,   1,23,-25,    4,   100f, 10)]
        public void ManualTest(
            int x1,int y1, int z1, 
            int x2, int y2, int z2, 
            int expectedLength,
            float hexEdgeLength, int trianglesPerEdge)
        {
            var start = new IntTriangularPos(x1,y1,z1);
            var end = new IntTriangularPos(x2,y2,z2);

            var triangleHeight = TriangularMath.GetTriangleHeight(hexEdgeLength / trianglesPerEdge);
            var startHexCoord = HexMath.DefineHex(start, triangleHeight, hexEdgeLength);
            var endHexCoord = HexMath.DefineHex(end, triangleHeight, hexEdgeLength);
            Assert.AreEqual(startHexCoord, endHexCoord, "input values not correct: points are in different hexes");

            var settings = MapSettings.CreateWithDefaultBorders(hexEdgeLength, trianglesPerEdge);
            using var map = new NavigationMap(settings, Allocator.TempJob);
            SetAllTrisInHexPassable(startHexCoord, map);

            using var collections = PrepareTriangularPathJobCollectionsCommand.Execute(Allocator.TempJob, new(startHexCoord, hexEdgeLength, trianglesPerEdge), map);
            var job = new ConstructTriangularPathJob()
            {
                CalculationData = collections.CalculationData,
                OpenedList = collections.OpenedList,
                PassabilityData = collections.PassabilityData,
                ResultList = collections.ResultList,
                PathCost = collections.PathCostReference
            };

            ChangeTrianglePathJobSetupDataCommand.Execute(ref job, collections, start, map);
            job.Start = start;
            job.End = end;

            job.RunByRef();
            foreach (var pos in job.ResultList)
            {
                TestContext.WriteLine(pos);
            }

            Assert.AreEqual(expectedLength, job.ResultList.Length);
        }

        private void SetAllTrisInHexPassable(int2 hexCoord, IUpdatableMap map)
        {
            var hexPos = new NavigationHexPosition(hexCoord, map);
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, map.TrianglesPerHexEdge))
            {
                map.UpdateCellPassability(tripos, new(true, int.MaxValue));
            }
        }
    }
}
