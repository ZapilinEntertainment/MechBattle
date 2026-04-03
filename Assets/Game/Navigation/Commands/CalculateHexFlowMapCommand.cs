using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using System;

namespace ZE.MechBattle.Navigation
{
    public static class CalculateHexFlowMapCommand
    {
        public class NativeCollectionsData : IDisposable
        {
            public SquaredHexTrianglesList<TriangleNavData> SetupData;
            public NativeArray<FlowFieldCellCalculationData> CalculationData;
            public NativeQueue<int> CalculationQueue;
            public NativeHashSet<int> QueuedPositions;

            private NativeArray<TriangleNavData> _setupDataArray;

            public NativeCollectionsData(Allocator allocator, IntTriangularPos triangularCenterPos, int trianglesRadius )
            {
                var coordsConverter = new TrianglesToIndexConverter(triangularCenterPos, trianglesRadius);
                _setupDataArray = new NativeArray<TriangleNavData>(coordsConverter.ArrayElementsCount, allocator);
                SetupData = new SquaredHexTrianglesList<TriangleNavData>( _setupDataArray, coordsConverter);

                CalculationQueue = new NativeQueue<int>(allocator);
                var hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(trianglesRadius);
                QueuedPositions = new NativeHashSet<int>(hexTrianglesCount / 2, allocator);
                CalculationData = new NativeArray<FlowFieldCellCalculationData>(SetupData.Length, allocator, NativeArrayOptions.UninitializedMemory);
            }

            public void Dispose()
            {
                _setupDataArray.Dispose();
                CalculationData.Dispose();
                CalculationQueue.Dispose();
                QueuedPositions.Dispose();
            }
        }

        private static readonly QueryParameters s_flowMapQueryParameters = NavigationConstants.GetGroundCastQueryParameters();
        private const float LOCK_PERCENT = NavigationConstants.NAV_OBSTACLES_LOCK_PERCENT;

        public static async Awaitable<HexFlowMap> ExecuteAsync(
            Allocator allocator,
            NavigationHexPosition hex, 
            INavigationCaster caster, 
            CancellationToken cancellationToken)
        {
            using var raycastData = await caster.CastHexAsync(hex.CenterPosWorld, s_flowMapQueryParameters, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }

            // TODO: move casting to own command
            var refinedData = RefineNavRaycastDataCommand.Execute(raycastData.AsReadOnly(), LOCK_PERCENT, caster);

            using var collections = new NativeCollectionsData(allocator, hex.TriangularCenterPos, caster.TrianglesPerHexEdge);   
            var data = collections.SetupData;
           
            foreach (var triangleKvp in refinedData)
            {
                var navdata = triangleKvp.Value;
                data.Set(triangleKvp.Key, navdata);
            }

            if (FlowMapCellData.STRUCTURE_SIZE * 6 * data.Length > 1024 * 900)
                throw new System.Exception("potential stack overflow");


            var resultingData = await PrepareAndCombineFlowMaps(collections, hex, caster, allocator, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return default;

            var accessMap = FormHexAccessMapCommand.Execute(resultingData.AsReadOnly(), hex, caster.TrianglesPerHexEdge);
            return new HexFlowMap(resultingData, accessMap);            
        }

        private static async Awaitable<NativeHashMap<IntTriangularPos, FlowMapCombinedCell>> PrepareAndCombineFlowMaps(
            NativeCollectionsData data, 
            NavigationHexPosition hexPos, 
            INavigationCaster caster,
            Allocator allocator,
            CancellationToken cancellationToken)
        {
            var setupData = data.SetupData;
            var calculationData = data.CalculationData;
            var length = setupData.Length;
            var coordsConverter = setupData.CoordsConverter;
            var radius = caster.TrianglesPerHexEdge;

            using var compositeMap = new CombinedFlowMapCellsStorage(length, setupData.CoordsConverter);
            var trianglesInHex = TriangularMath.GetTrianglesCountInHex(radius);
            var hexTriangleIndices = new int[trianglesInHex];
            var ti = 0;
            foreach (var hexTrianglePos in new HexTrianglesEnumerator(hexPos, radius))
            {
                var index = coordsConverter.TriangularToIndex(hexTrianglePos);
                hexTriangleIndices[ti++] = index;
            }


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
                while (!handle.IsCompleted)
                {
                    await Awaitable.NextFrameAsync();
                }
                handle.Complete();

                if (cancellationToken.IsCancellationRequested)
                    return default;

                for (var i = 0; i < trianglesInHex; i++)
                {
                    var index = hexTriangleIndices[i];

                    var defaultData = setupData[index];
                    if (!defaultData.IsValid)
                        continue;

                    var calculatedData = calculationData[index];
                    var cellData = new FlowMapCellData(direction: calculatedData.FlowDirection, exitDistance: (ushort)calculatedData.IntegrationValue);
                    compositeMap.SetValue(edge, index, cellData);
                }
            }

            var resultingData = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(caster.HexTrianglesCount, allocator);   
            for (var i = 0; i < trianglesInHex; i++)
            {
                var index = hexTriangleIndices[i];
                var triangleSetupData = setupData[index];
                if (!triangleSetupData.IsValid)
                    continue;

                var compositeCell = compositeMap.GetCombinedCell(index, triangleSetupData);
                resultingData.Add(coordsConverter.IndexToTriangular(index), compositeCell);
            }

            return resultingData;
        }
    }
}
