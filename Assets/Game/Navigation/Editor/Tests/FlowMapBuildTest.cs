using System;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

namespace ZE.MechBattle.Navigation.Tests
{
    public class FlowMapBuildTest
    {
        private class TestData : IDisposable
        {
            public CombinedFlowMapCellsStorage CombinedMap;
            public NativeArray<IntTriangularPos> HexTriangles;

            public void Dispose()
            {
                CombinedMap.Dispose();
                HexTriangles.Dispose();
            }
        }

        private async Awaitable<TestData> PrepareTestData(int hexX, int hexY, float hexEdge, int trianglesPerEdge)
        {
            var data = new TestData();

            var triangleHeight = (hexEdge / trianglesPerEdge) * NavigationConstants.SQRT_OF_THREE_HALVED;
            var hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(trianglesPerEdge);
            var hexPos = new NavigationHexPosition(hexX, hexY, hexEdge, triangleHeight);

            var allocator = Allocator.Persistent;
            using var calculationCollections = FlowFieldCalculationCollections.CreateCollection(allocator, hexPos, MapSettings.CreateWithDefaultBorders(hexEdge, trianglesPerEdge));

            data.HexTriangles = PrepareTrianglesData(ref calculationCollections.PassabilityData, hexPos, hexTrianglesCount, trianglesPerEdge);

            using var CalculationQueue = new NativeQueue<int>(allocator);
            using var QueuedPositions = new NativeHashSet<int>(hexTrianglesCount / 2, allocator);

            var compositeStorage = new CombinedFlowMapCellsStorage(hexTrianglesCount, calculationCollections.PassabilityData.GetCoordsConverter());
            data.CombinedMap = compositeStorage;
            
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                var job = new GenerateExitEdgeFlowFieldJob()
                {
                    PassabilityData = calculationCollections.PassabilityData,
                    CalculationData = calculationCollections.CalculationData,
                    HexData = hexPos,
                    CalculationQueue = CalculationQueue,
                    QueuedPositions = QueuedPositions,
                    ExitEdge = edge,
                    TrianglesPerEdge = trianglesPerEdge
                };
                var handle = job.ScheduleByRef();
                while (!handle.IsCompleted)
                {
                    await Awaitable.NextFrameAsync();
                }
                handle.Complete();

                for (var index = 0; index < hexTrianglesCount; index++)
                {
                    var cellSetupData = job.PassabilityData[index];
                    var cellCalculatedData = job.CalculationData[index];
                    Assert.IsTrue(cellCalculatedData.IsCalculated, "cell not calculated");
                    Assert.IsTrue(cellSetupData.IsPassable, "cell not passable");

                    var flowDirection = cellCalculatedData.FlowDirection;
                    Assert.IsTrue(flowDirection >=0 && flowDirection < 12, "flow direction incorrect");
                    Assert.IsTrue(cellCalculatedData.IntegrationValue >=0 && cellCalculatedData.IntegrationValue < ushort.MaxValue, "distance incorrect");

                    var cellData = new FlowMapCellData(flowDirection, (ushort)cellCalculatedData.IntegrationValue);
                    Assert.AreEqual(true, cellSetupData.IsPassable, "encoding unsuccessful");

                    compositeStorage.SetValue(edge, index, cellData);
                }
            }

            return data;
        }

        private NativeArray<IntTriangularPos> PrepareTrianglesData(
            ref FlattenedHexList<CellPassabilityData> setupData, 
            NavigationHexPosition hexPos,
            int trianglesInHex,
            int hexRadius)
        {
            var list = new NativeArray<IntTriangularPos>(trianglesInHex, Allocator.Persistent);
            GetTrianglesInHexCommand.Execute(hexPos.InnerRingTopValleyTriangle, hexRadius, list);

            var passableTriangleData = CellPassabilityData.CreateDefaultData(true);
            foreach (var tripos in list)
            {
                setupData[tripos] = passableTriangleData;
            }
            return list;
        }    
    }
}
