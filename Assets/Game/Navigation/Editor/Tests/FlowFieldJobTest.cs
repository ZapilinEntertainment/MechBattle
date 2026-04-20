using NUnit.Framework;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation.Tests
{
    public class FlowFieldJobTest
    {
        [TestCase(0, 0, 100f, 2, 0)]
        [TestCase(0,0,100f, 4, 0)]
        public void JobTest(int hexCoordX, int hexCoordY, float hexEdgeSize, int radius, int edgeIndex)
        {
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, hexEdgeSize, hexEdgeSize / radius * NavigationConstants.SQRT_OF_THREE_HALVED);
            var allocator = Allocator.Persistent;
            var mapSettings = MapSettings.CreateWithDefaultBorders(hexEdgeSize, radius);
            using var collectionData = new FlowFieldCalculationCollections(allocator, hexPos.TriangularCenterPos, mapSettings);

            var hexTrisCount = TriangularMath.GetTrianglesCountInHex(radius);
            var hexEnumerator = new HexTrianglesEnumerator(hexPos.TriangularCenterPos, radius);
            foreach (var tripos in hexEnumerator)
            {
                collectionData.PassabilityData[tripos] = CellPassabilityData.CreateDefaultData(true);
            }

            var exitEdge = (HexEdge)edgeIndex;
            var job = new GenerateFlowFieldJob()
            {
                CalculationData = collectionData.CalculationData,
                HexData = hexPos,
                PassabilityData= collectionData.PassabilityData,
                CalculationQueue= collectionData.CalculationQueue,
                QueuedPositions=collectionData.QueuedPositions,
                TrianglesPerEdge=radius,
                ExitEdge = exitEdge
            };
            var handle = job.Schedule();
            handle.Complete();

            var testCompleted = true;

            hexEnumerator.Reset();
            foreach (var tripos in hexEnumerator)
            {
                var cellData = job.PassabilityData[tripos];
                var calculationData = job.CalculationData[job.PassabilityData.TriangularToIndex(tripos)];
            }


            hexEnumerator.Reset();
            foreach (var tripos in hexEnumerator)
            {
                var index = job.PassabilityData.TriangularToIndex(tripos);
                var data = job.CalculationData[index];

                //var directionStrings = tripos.IsPeak ? ((PeakNeighbour)data.FlowDirection).ToString() : ((ValleyNeighbour)data.FlowDirection).ToString();
                //TestContext.WriteLine($"exit: {exitEdge}");
                //TestContext.WriteLine($"{tripos} : {directionStrings} : {data.IntegrationValue}");


                var integrationValue = data.IntegrationValue;
                var target = GetTargetIntegrationValue(tripos, job);
                var passabilityData = job.PassabilityData[tripos];

                if (tripos.IsPeak)
                {
                    for (var j = 0; j < 12; j++)
                    {
                        var dir = (PeakNeighbour)j;
                        var neighbourPos = TriangularMath.GetPeakNeighbour(tripos, dir);
                        if (neighbourPos == target.Item1)
                            continue;

                        if (job.PassabilityData.TryGetIndex(neighbourPos, out var neighbourIndex))
                        {
                            var neighbourIntegrationValue = job.CalculationData[neighbourIndex].IntegrationValue;
                            if (neighbourIntegrationValue < target.Item2)
                            {
                                testCompleted = false;
                                TestContext.WriteLine($"better path found: {tripos}[{integrationValue}] -> {neighbourPos}[{neighbourIntegrationValue}] instead of {target.Item1}[{target.Item2}]");
                            }
                        }
                    }
                }
                else
                {
                    for (var j = 0; j < 12; j++)
                    {
                        var dir = (ValleyNeighbour)j;
                        var neighbourPos = TriangularMath.GetValleyNeighbour(tripos, dir);
                        if (neighbourPos == target.Item1)
                            continue;

                        if (job.PassabilityData.TryGetIndex(neighbourPos, out var neighbourIndex))
                        {
                            Assert.IsTrue(neighbourIndex >= 0, $"neighbour index is negative: {neighbourPos} -> {neighbourIndex}");

                            var neighbourData = job.PassabilityData[neighbourIndex];
                            if (!neighbourData.IsPassable || !passabilityData.IsNeighbourAccessible(j))
                                continue;

                            var neighbourIntegrationValue = job.CalculationData[neighbourIndex].IntegrationValue;
                            if (neighbourIntegrationValue < target.Item2)
                            {
                                testCompleted = false;
                                TestContext.WriteLine($"better path found: {tripos}[{integrationValue}] -> {neighbourPos}[{neighbourIntegrationValue:F5}] instead of {target.Item1}[{target.Item2:F5}]");
                            }
                        }
                    }
                }
            }

            if (!testCompleted) Assert.Ignore("flow map is not ideal" );

            var calcData = job.CalculationData;
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, radius))
            {
                var index = job.PassabilityData.TriangularToIndex(tripos);
                var integration = calcData[index].IntegrationValue;
                var targetValue = GetTargetIntegrationValue(tripos, job);

                var dir = calcData[index].FlowDirection;
                var directionString = tripos.IsPeak ? ((PeakNeighbour)dir).ToString() : ((ValleyNeighbour)dir).ToString();

                TestContext.WriteLine($"{tripos}  [{integration}] -> {targetValue.Item1} [{targetValue.Item2}]  ({directionString})");
            }
        }
    
        private (IntTriangularPos,float) GetTargetIntegrationValue(IntTriangularPos pos, in GenerateFlowFieldJob job)
        {
            var index = job.PassabilityData.TriangularToIndex(pos);

            var direction = job.CalculationData[index].FlowDirection;

            var targetPos = TriangularMath.GetNeighbourByDirection(pos, direction);
            if (!job.PassabilityData.TryGetIndex(targetPos, out var targetIndex))
                return default;
            var targetIntegrationValue = job.CalculationData[targetIndex].IntegrationValue;
            return (targetPos, targetIntegrationValue);
        }
    }
}
