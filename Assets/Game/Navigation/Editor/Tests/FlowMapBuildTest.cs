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

            var coordsConverter = new TrianglesToIndexConverter(hexPos.TriangularCenterPos, trianglesPerEdge);
            var setupData = new NativeArray<TriangleNavData>(coordsConverter.ArrayElementsCount, allocator);
            var squaredArray = new SquaredHexTrianglesList<TriangleNavData>(setupData, coordsConverter);

            var length = setupData.Length;
            var calculationData = new NativeArray<FlowFieldCellCalculationData>(length, allocator);

            data.HexTriangles = PrepareTrianglesData(squaredArray, hexPos, hexTrianglesCount, trianglesPerEdge);

            var CalculationQueue = new NativeQueue<int>(allocator);
            var QueuedPositions = new NativeHashSet<int>(hexTrianglesCount / 2, allocator);

            var compositeStorage = new CombinedFlowMapCellsStorage(length, coordsConverter);
            data.CombinedMap = compositeStorage;
            
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                var job = new GenerateFlowFieldJob()
                {
                    SetupData = squaredArray,
                    CalculationData = calculationData,
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

                //CheckEdgeDirections(calculationData, coordsConverter, edge);

                var validTrisCount = 0;
                for (var index = 0; index < length; index++)
                {
                    var cellSetupData = setupData[index];
                    if (!cellSetupData.IsValid)
                    {
                        var tripos = coordsConverter.IndexToTriangular(index);
                        var cell = FlowMapCellData.FormBlockedCell(edge, tripos, (ushort)calculationData[index].IntegrationValue);
                        compositeStorage.SetValue(edge, index, cell);
                        continue;
                    }

                    validTrisCount++;
                    var cellCalculatedData = calculationData[index];
                    Assert.IsTrue(cellCalculatedData.IsCalculated, "cell not calculated");
                    Assert.IsTrue(cellSetupData.IsPassable, "cell not passable");

                    var flowDirection = cellCalculatedData.FlowDirection;
                    Assert.IsTrue(flowDirection >=0 && flowDirection < 12, "flow direction incorrect");
                    Assert.IsTrue(cellCalculatedData.IntegrationValue >=0 && cellCalculatedData.IntegrationValue < ushort.MaxValue, "distance incorrect");

                    var cellData = new FlowMapCellData(flowDirection, (ushort)cellCalculatedData.IntegrationValue);
                    Assert.AreEqual(true, cellSetupData.IsPassable, "encoding unsuccessful");

                    compositeStorage.SetValue(edge, index, cellData);
                }

                Assert.AreEqual(hexTrianglesCount, validTrisCount, "some hex tris were not valid");
            }

            setupData.Dispose();
            calculationData.Dispose();
            CalculationQueue.Dispose();
            QueuedPositions.Dispose();

            return data;
        }

        private NativeArray<IntTriangularPos> PrepareTrianglesData(
            SquaredHexTrianglesList<TriangleNavData> setupData, 
            NavigationHexPosition hexPos,
            int trianglesInHex,
            int hexRadius)
        {
            var list = new NativeArray<IntTriangularPos>(trianglesInHex, Allocator.Persistent);
            GetTrianglesInHexCommand.Execute(hexPos.InnerRingTopTriangle, hexRadius, list);

            var passableTriangleData = TriangleNavData.CreateDefaultData(true);
            foreach (var tripos in list)
            {
                setupData.Set(tripos, passableTriangleData);
            }
            return list;
        }

        // checking ideal no-obstacles flow map
        private void CheckEdgeDirections(
            NativeArray<FlowFieldCellCalculationData> calculationData, 
            TrianglesToIndexConverter coordsConverter,
            HexEdge exitEdge)
        {

            var length = calculationData.Length;
            for (var i = 0; i < length; i++)
            {
                //if (!setupData[i].IsValid)  continue;

                var pos = coordsConverter.IndexToTriangular(i);
                var flowDir = calculationData[i ].FlowDirection;

                var peakDir = (int)exitEdge.ToNeighbourDirectionFromPeak();
                var valleyDir = (int)exitEdge.ToNeighbourDirectionFromValley();

                //TestContext.WriteLine($"{exitEdge}: {i}: direction is {(pos.IsPeak ? "peak" : "valley")}.{(PeakNeighbour)flowDir}");

                //if (pos.IsPeak)
                //    Assert.AreEqual(flowDir, peakDir, $"{exitEdge}: {i}: direction is {(pos.IsPeak ? "peak" : "valley")}.{(PeakNeighbour)flowDir}");
                //else
                //    Assert.AreEqual(flowDir, valleyDir, $"{exitEdge}: {i}: direction is {(pos.IsPeak ? "peak" : "valley")}.{(ValleyNeighbour)flowDir}");
            }
        }
    
    }
}
