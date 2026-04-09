using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation.Tests
{

    public class CombinedFlowMapTest
    {
        [TestCase(0, 0, 100f, 1)]
        [TestCase(0, 0, 100f, 2)]
        [TestCase(0, 0, 100f, 4)]
        [TestCase(3, -3, 100f, 8)]
        [MethodImpl(MethodImplOptions.NoOptimization)]
        public void CheckCombinedPassability(int hexCoordX, int hexCoordY, float hexEdge, int radius)
        {
            var hexPos = new NavigationHexPosition(hexCoordX, hexCoordY, hexEdge, hexEdge / radius * NavigationConstants.SQRT_OF_THREE_HALVED);

            var allocator = Allocator.Persistent;
            using var collectionsData = new FlowFieldCalculationCollections(allocator, hexPos.TriangularCenterPos, radius);
            var setupData = collectionsData.SetupData;
            var triangleData = TriangleNavData.CreateDefaultData(true);
            foreach (var triangle in new HexTrianglesEnumerator(hexPos, radius))
            {
                setupData.Set(triangle, triangleData);
            }

            if (FlowMapCellData.STRUCTURE_SIZE * 6 * setupData.Length > 1024 * 900)
                Assert.Fail("potential stack overflow");


            using var resultingData = PrepareAndCombineFlowMaps(collectionsData, hexPos, radius, allocator);
            //var accessMap = FormHexAccessMapCommand.Execute(resultingData.AsReadOnly(), hex, caster.TrianglesPerHexEdge);
            //return new HexFlowMap(resultingData, accessMap);
        }

        private NativeHashMap<IntTriangularPos, FlowMapCombinedCell> PrepareAndCombineFlowMaps(
           FlowFieldCalculationCollections data,
           NavigationHexPosition hexPos,
           int radius,
           Allocator allocator)
        {
            var setupData = data.SetupData;
            var calculationData = data.CalculationData;
            var length = setupData.Length;
            var coordsConverter = setupData.CoordsConverter;

            using var compositeMap = new CombinedFlowMapCellsStorage(length, setupData.CoordsConverter);
            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(radius);

            // indices of triangles only in hex (squared array have also outside ones)
            var hexTriangleIndices = new int[trianglesInHex];
            var ti = 0;
            foreach (var hexTrianglePos in new HexTrianglesEnumerator(hexPos, radius))
            {
                var index = coordsConverter.TriangularToIndex(hexTrianglePos);
                hexTriangleIndices[ti++] = index;
                Assert.IsTrue(setupData[index].IsPassable, $"{hexTrianglePos} is not passable by default");
            }

            Assert.IsTrue(radius != 0, "radius is zero");
            Assert.AreEqual(radius * radius * 6, trianglesInHex, "triangles in hex count wrong");
            TestContext.WriteLine($"{trianglesInHex} tris in hex");

            var settedIndices = new HashSet<int2>();
            for (var e = 0; e < 6; e++)
            {
                var edge = (HexEdge)e;
                var job = new GenerateFlowFieldJob()
                {
                    SetupData = setupData,
                    CalculationData = calculationData,
                    HexData = hexPos,
                    CalculationQueue = data.CalculationQueue,
                    QueuedPositions = data.QueuedPositions,
                    ExitEdge = edge,
                    TrianglesPerEdge = radius
                };
                var handle = job.ScheduleByRef();
                handle.Complete();
                
                for (var i = 0; i < trianglesInHex; i++)
                {
                    var index = hexTriangleIndices[i];

                    Assert.IsTrue(job.SetupData[index].IsPassable, "setup data was changed");

                    var defaultData = setupData[index];
                    Assert.IsTrue(defaultData.IsValid);

                    var calculatedData = job.CalculationData[index];
                    Assert.IsTrue(calculatedData.FlowDirection >= 0 && calculatedData.FlowDirection < ushort.MaxValue,
                        $"incorrect flow direction at {edge} {coordsConverter.IndexToTriangular(index)} = {calculatedData.FlowDirection}");

                    if (!defaultData.IsPassable)
                        Debug.Log($"{coordsConverter.IndexToTriangular(index)} is not passable when calculate cell data");
                    var cellData = new FlowMapCellData(calculatedData.FlowDirection, (ushort)calculatedData.IntegrationValue);
                    compositeMap.SetValue(edge, index, cellData);

                    settedIndices.Add(new((int)edge, index));
                    //Debug.Log($"handled: {edge}:{index}");
                }
            }           

            TestContext.WriteLine($"handles indices count: {settedIndices.Count}");

            for (var i = 0; i < trianglesInHex; i++)
            {
                var index = hexTriangleIndices[i];
                for (var e = 0; e < 6; e++)
                {
                    Assert.IsTrue(settedIndices.Contains(new(e, index)), $"{(HexEdge)e}:{index} not handled before!");
                }
            }


            var resultingData = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(trianglesInHex, allocator);
            for (var i = 0; i < trianglesInHex; i++)
            {
                var index = hexTriangleIndices[i];
                var triangleSetupData = setupData[index];

                if (!triangleSetupData.IsValid)
                    continue;

                Assert.IsTrue(triangleSetupData.IsPassable, "setup data is not passable");
            }

            return resultingData;
        }
    }
}
