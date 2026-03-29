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
            public SquaredHexTrianglesList<FlowFieldCellSetupData> SetupData;
            public NativeArray<FlowFieldCellCalculationData> CalculationData;
            public NativeQueue<int> CalculationQueue;
            public NativeHashSet<int> QueuedPositions;

            public NativeCollectionsData(Allocator allocator, IntTriangularPos triangularCenterPos, int trianglesRadius )
            {
                SetupData = new SquaredHexTrianglesList<FlowFieldCellSetupData>(triangularCenterPos, trianglesRadius, allocator);
                CalculationQueue = new NativeQueue<int>(allocator);
                var hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(trianglesRadius);
                QueuedPositions = new NativeHashSet<int>(hexTrianglesCount / 2, allocator);
                CalculationData = new NativeArray<FlowFieldCellCalculationData>(SetupData.Length, allocator, NativeArrayOptions.UninitializedMemory);
            }

            public void Dispose()
            {
                SetupData.Dispose();
                CalculationData.Dispose();
                CalculationQueue.Dispose();
                QueuedPositions.Dispose();
            }
        }

        private static readonly QueryParameters s_flowMapQueryParameters = NavigationConstants.GetGroundCastQueryParameters();
        private const float LOCK_PERCENT = NavigationConstants.NAV_OBSTACLES_LOCK_PERCENT;
        private const float DEFAULT_ENTRANCE_COST = 1f;

        public static async Awaitable<HexFlowMap> ExecuteAsync(
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

            using var collections = new NativeCollectionsData(Allocator.Persistent, hex.TriangularCenterPos, caster.TrianglesPerHexEdge);   
            var data = collections.SetupData;
           
            foreach (var triangleKvp in refinedData)
            {
                data.Set(triangleKvp.Key, new()
                {
                    EntranceCost = DEFAULT_ENTRANCE_COST,
                    IsValid = true
                });
            }

            if (FlowMapCellData.STRUCTURE_SIZE * 6 * data.Length > 1024 * 900)
                throw new System.Exception("potential stack overflow");


            var resultingData = await PrepareAndCombineFlowMaps(collections, hex, caster, Allocator.Persistent, cancellationToken);
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

                if (!setupData[index].IsPassable) 
                    Debug.Log($"{hexTrianglePos} is not passable by default");
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
                    FlowMapCellData cellData;
                    if (calculatedData.FlowDirection < 0)
                    {
                        var tripos = coordsConverter.IndexToTriangular(index);
                        cellData = FlowMapCellData.FormBlockedCell(edge, tripos, (ushort)calculatedData.IntegrationValue);
                    }
                        
                    else
                    {
                        cellData = new(defaultData.IsPassable, calculatedData.FlowDirection, (ushort)calculatedData.IntegrationValue);
                    }                        
                    
                    compositeMap.SetValue(edge, index, cellData);
                }
            }

            var resultingData = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(caster.HexTrianglesCount, allocator);   
            for (var i = 0; i < trianglesInHex; i++)
            {
                var index = hexTriangleIndices[i];

                if (!setupData[index].IsValid)
                    continue;
                var compositeCell = compositeMap.GetCombinedCell(index);
                resultingData.Add(coordsConverter.IndexToTriangular(index), compositeCell);
            }

            return resultingData;
        }
    }
}
