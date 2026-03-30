using NUnit.Framework;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

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
            using var collectionData = new CalculateHexFlowMapCommand.NativeCollectionsData(allocator, hexPos.TriangularCenterPos, radius);

            var hexTrisCount = TriangularMath.GetTrianglesCountInHex(radius);
            using var hexTriangles = new NativeArray<IntTriangularPos>(hexTrisCount, allocator, NativeArrayOptions.UninitializedMemory);
            GetTrianglesInHexCommand.Execute(hexPos.InnerRingTopTriangle, radius, hexTriangles);

            for (var i = 0; i < hexTrisCount; i++)
            {
                var pos = hexTriangles[i];
                collectionData.SetupData.Set(pos, TriangleNavData.CreateDefaultData(true));
            }

            var exitEdge = (HexEdge)edgeIndex;
            var job = new GenerateFlowFieldJob()
            {
                CalculationData = collectionData.CalculationData,
                HexData = hexPos,
                SetupData= collectionData.SetupData,
                CalculationQueue= collectionData.CalculationQueue,
                QueuedPositions=collectionData.QueuedPositions,
                TrianglesPerEdge=radius,
                ExitEdge = exitEdge
            };
            var handle = job.Schedule();
            handle.Complete();

            var testCompleted = true;
            var coordsConverter = collectionData.SetupData.CoordsConverter;

            for (var i = 0; i < hexTrisCount; i++)
            {
                var tripos = hexTriangles[i];
                var index = coordsConverter.TriangularToIndex(tripos);
                var data = job.CalculationData[index];

                //var directionStrings = tripos.IsPeak ? ((PeakNeighbour)data.FlowDirection).ToString() : ((ValleyNeighbour)data.FlowDirection).ToString();
                //TestContext.WriteLine($"exit: {exitEdge}");
                //TestContext.WriteLine($"{tripos} : {directionStrings} : {data.IntegrationValue}");


                var integrationValue = data.IntegrationValue;                
                var target = GetTargetIntegrationValue(tripos, job);

                if (tripos.IsPeak)
                {

                    for (var j = 0; j < 12; j++)
                    {
                        var dir = (PeakNeighbour)j;
                        var neighbourPos = TriangularMath.GetPeakNeighbour(tripos, dir);
                        if (neighbourPos == target.Item1)
                            continue;

                        if (coordsConverter.TryConvertToIndex(neighbourPos, out var neighbourIndex))
                        {
                            if (!job.SetupData.IsIndexValid(neighbourIndex))
                                continue;

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

                        if (coordsConverter.TryConvertToIndex(neighbourPos, out var neighbourIndex))
                        {
                            if (!job.SetupData.IsIndexValid(neighbourIndex))
                                continue;

                            var neighbourIntegrationValue = job.CalculationData[neighbourIndex].IntegrationValue;
                            if (neighbourIntegrationValue < target.Item2)
                            {
                                testCompleted = false;
                                TestContext.WriteLine($"better path found: {tripos}[{integrationValue}] -> {neighbourPos}[{neighbourIntegrationValue}] instead of {target.Item1}[{target.Item2}]");
                            }
                        }
                    }
                }
            }

            Assert.IsTrue( testCompleted );

            var calcData = job.CalculationData;
            for (var i = 0; i < hexTrisCount; i++)
            {
                var pos = hexTriangles[i];
                var index = coordsConverter.TriangularToIndex(pos);
                var integration = calcData[index].IntegrationValue;
                var targetValue = GetTargetIntegrationValue(pos, job);

                var dir = calcData[index].FlowDirection;
                var directionString = pos.IsPeak ? ((PeakNeighbour)dir).ToString() : ((ValleyNeighbour)dir).ToString();

                TestContext.WriteLine($"{pos}  [{integration}] -> {targetValue.Item1} [{targetValue.Item2}]  ({directionString})");
            }
        }
    
        private (IntTriangularPos,float) GetTargetIntegrationValue(IntTriangularPos pos, in GenerateFlowFieldJob job)
        {
            var coordsConverter = job.SetupData.CoordsConverter;
            var index = coordsConverter.TriangularToIndex(pos);

            var direction = job.CalculationData[index].FlowDirection;

            var targetPos = TriangularMath.GetNeighbourByDirection(pos, direction);
            if (!coordsConverter.TryConvertToIndex(targetPos, out var targetIndex))
                return default;
            var targetIntegrationValue = job.CalculationData[targetIndex].IntegrationValue;
            return (targetPos, targetIntegrationValue);
        }
    }
}
