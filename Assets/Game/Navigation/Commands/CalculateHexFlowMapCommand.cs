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
       

        private class NativeCollectionsData : IDisposable
        {
            public SquaredHexTrianglesList<FlowFieldCellSetupData> SetupData;
            public NativeArray<FlowFieldCellCalculationData> CalculationData;
            public NativeQueue<int> CalculationQueue;
            public NativeHashSet<int> QueuedPositions;

            public NativeCollectionsData(Allocator allocator, IntTriangularPos triangularCenterPos, INavigationCaster caster)
            {
                SetupData = new SquaredHexTrianglesList<FlowFieldCellSetupData>(triangularCenterPos, caster.TrianglesPerHexEdge, allocator);
                CalculationQueue = new NativeQueue<int>(allocator);
                QueuedPositions = new NativeHashSet<int>(caster.HexTrianglesCount / 2, allocator);
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

            using var collections = new NativeCollectionsData(Allocator.Persistent, hex.TriangularCenterPos, caster);   
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
            NavigationHexPosition hex, 
            INavigationCaster caster,
            Allocator allocator,
            CancellationToken cancellationToken)
        {
            var setupData = data.SetupData;
            var calculationData = data.CalculationData;
            var length = setupData.Length;

            using var disposableArray = new DisposableArray(length * 6);
            var combinedData = disposableArray.Values;

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                var job = new GenerateFlowFieldJob()
                {
                    SetupData = setupData,
                    CalculationData = calculationData,
                    HexData = hex,
                    CalculationQueue = data.CalculationQueue,
                    QueuedPositions = data.QueuedPositions,
                    ExitEdge = edge,
                    TrianglesPerEdge = caster.TrianglesPerHexEdge
                };
                var handle = job.ScheduleByRef();
                while (!handle.IsCompleted)
                {
                    await Awaitable.NextFrameAsync();
                }
                handle.Complete();
                Debug.Log($"{edge} completed");

                if (cancellationToken.IsCancellationRequested)
                    return default;

                for (var j = 0; j < length; j++)
                {
                    var defaultData = setupData[j];
                    if (defaultData.IsValid)
                        continue;

                    var calculatedData = calculationData[j];
                    combinedData[i * length + j] = new FlowMapCellData(defaultData.IsPassable, calculatedData.FlowDirection, (ushort)calculatedData.IntegrationValue).Value;
                }
            }

            var resultingData = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(caster.HexTrianglesCount, allocator);
            var coordsConverter = setupData.CoordsConverter;
            var cellData = new FlowMapCellData[6];
            for (var i = 0; i < length; i++)
            {
                if (!setupData[i].IsValid)
                    continue;
                for (var j = 0; j < 6; j++)
                {
                    cellData[j] = new(combinedData[j * length + i]);
                }
                resultingData.Add(coordsConverter.IndexToTriangular(i), new(cellData));
            }

            return resultingData;
        }
    }
}
