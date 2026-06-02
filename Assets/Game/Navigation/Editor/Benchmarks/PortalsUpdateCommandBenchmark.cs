using NUnit.Framework;
using Unity.Collections;
using Unity.Mathematics;
using Unity.PerformanceTesting;

namespace ZE.MechBattle.Navigation.Tests
{
    public class PortalsUpdateCommandBenchmark
    {
        public NavigationMap PrepareTestingMap(Allocator allocator, int trianglesPerEdge)
        {
            var mapSettings = MapSettings.CreateWithDefaultBorders(100f, trianglesPerEdge);
            var map = new NavigationMap(mapSettings, allocator);

            // hex with edge rows per each edge
            foreach (var tripos in new HexTrianglesEnumerator(IntTriangularPos.zero, trianglesPerEdge + 1))
            {
                map.UpdateCellPassability(tripos, default);
            }

            return map;
        }

        [TestCase(10), Performance]
        public void UpdatePortalsBenchmarkTest(int trianglesPerEdge)
        {
            using var map = PrepareTestingMap(Allocator.TempJob, trianglesPerEdge);
            Measure.Method(() => CalculateHexExitsCommand.Execute(map, int2.zero, HexEdge.Top, new()))
                .WarmupCount(3)
                .MeasurementCount(12 * 5)                
                .Run();
        }
    }
}
