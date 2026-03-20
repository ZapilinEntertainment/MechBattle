using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using System;

namespace ZE.MechBattle.Navigation
{
    public static class CalculateHexFlowMapCommand
    {
        private class NativeCollectionsData : IDisposable
        {
            public SquaredHexTrianglesList<FlowFieldCellCalculationData> SquaredHexData;
            public NativeQueue<int> CalculationQueue;
            public NativeHashSet<int> QueuedPositions;

            public NativeCollectionsData(Allocator allocator, IntTriangularPos triangularCenterPos, INavigationCaster caster)
            {
                var data = new SquaredHexTrianglesList<FlowFieldCellCalculationData>(triangularCenterPos, caster.TrianglesPerHexEdge, Allocator.Persistent);
                var calculationQueue = new NativeQueue<int>(Allocator.Persistent);
                var queuedPositions = new NativeHashSet<int>(caster.HexTrianglesCount / 2, Allocator.Persistent);
            }

            public void Dispose()
            {
                SquaredHexData.Dispose();
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
            var raycastData = await caster.CastHexAsync(hex.CenterPos, s_flowMapQueryParameters, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                raycastData.Dispose();
                return default;
            }
            Debug.Log($"hex casted : {raycastData.Length}");

            // TODO: move casting to own command
            var refinedData = RefineNavRaycastDataCommand.Execute(raycastData.AsReadOnly(), LOCK_PERCENT, caster);
            var collections = new NativeCollectionsData(Allocator.Persistent, hex.TriangularCenterPos, caster);   
            var data = collections.SquaredHexData;
           
            foreach (var triangleKvp in refinedData)
            {
                data.Set(triangleKvp.Key, new()
                {
                    EntranceCost = DEFAULT_ENTRANCE_COST,
                    IsValid = true
                });
            }
            Debug.Log($"data refined : {data.Length}");

            if (FlowMapCellData.STRUCTURE_SIZE * 6 * data.Length > 1024 * 900)
                throw new System.Exception("potential stack overflow");

            var resultingData = PrepareAndCombineFlowMaps(collections, hex, caster, Allocator.Persistent);
            collections.Dispose();
            Debug.Log($"flow maps combined: {resultingData.Count}");

            var accessMap = FormHexAccessMapCommand.Execute(resultingData.AsReadOnly(), hex, caster.TrianglesPerHexEdge);
            Debug.Log("edges access map ready");
            return new HexFlowMap(resultingData, accessMap);            
        }

        private static NativeHashMap<IntTriangularPos, FlowMapCombinedCell> PrepareAndCombineFlowMaps(
            NativeCollectionsData data, 
            NavigationHexPosition hex, 
            INavigationCaster caster,
            Allocator allocator)
        {
            var hexData = data.SquaredHexData;
            var length = hexData.Length;
            Span<int> combinedData = stackalloc int[length * 6];

            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                var job = new GenerateFlowFieldJob()
                {
                    Data = hexData,
                    HexData = hex,
                    CalculationQueue = data.CalculationQueue,
                    QueuedPositions = data.QueuedPositions,
                    ExitEdge = edge,
                    TrianglesPerEdge = caster.TrianglesPerHexEdge
                };

                for (var j = 0; j < length; j++)
                {
                    var resultData = hexData[j];
                    if (resultData.IsValid)
                        continue;
                    combinedData[j * i] = new FlowMapCellData(resultData.IsPassable, resultData.FlowDirection, (int)resultData.IntegrationValue).Value;
                }
            }

            var resultingData = new NativeHashMap<IntTriangularPos, FlowMapCombinedCell>(caster.HexTrianglesCount, allocator);
            var coordsConverter = hexData.CoordsConverter;
            var cellData = new FlowMapCellData[6];
            for (var i = 0; i < length; i++)
            {
                if (!hexData[i].IsValid)
                    continue;
                for (var j =0; j < 6; j++)
                {
                    cellData[j] = new(combinedData[i * 6]);
                }
                resultingData.Add(coordsConverter.IndexToTriangular(i), new(cellData));
            }
            return resultingData;
        }
    }
}
