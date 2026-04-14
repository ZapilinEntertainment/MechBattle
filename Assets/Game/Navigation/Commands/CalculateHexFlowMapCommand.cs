using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public static class CalculateHexFlowMapCommand
    {
        private static readonly QueryParameters s_flowMapQueryParameters = NavigationConstants.GetWalkableCastQueryParameters();
        private const float LOCK_PERCENT = NavigationConstants.NAV_OBSTACLES_LOCK_PERCENT;

        public static FlowFieldCalculationCollections PrepareCalculationCollections(Allocator allocator, NavigationHexPosition hexPos, int trianglePerEdge) =>
            new FlowFieldCalculationCollections(allocator, hexPos.TriangularCenterPos, trianglePerEdge);

        public static async Awaitable<HexFlowMap> ExecuteAsync(
            Allocator allocator,
            NavigationHexPosition hex, 
            INavigationCaster caster, 
            CancellationToken cancellationToken)
        {
            using var collections = PrepareCalculationCollections(Allocator.TempJob, hex, caster.TrianglesPerHexEdge);   
            return await ExecuteAsyncWithCachedCollections(allocator, hex, caster, collections, cancellationToken);         
        }

        public static async Awaitable<HexFlowMap> ExecuteAsyncWithCachedCollections(
            Allocator allocator,
            NavigationHexPosition hexPos,
            INavigationCaster caster,
            FlowFieldCalculationCollections collections,
            CancellationToken cancellationToken)
        {
            await caster.CastHexAsync(hexPos, s_flowMapQueryParameters, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }

            // TODO: move casting to own command
            //var refinedData = RefineNavRaycastDataCommand.Execute(hexPos, caster.Results), LOCK_PERCENT, caster);
            NativeHashMap<IntTriangularPos, TriangleNavData> refinedData = default;

            var data = collections.SetupData;

            foreach (var triangleKvp in refinedData)
            {
                var navdata = triangleKvp.Value;
                data.Set(triangleKvp.Key, navdata);
            }


            var resultingData = await CombineFlowMapsCommand.ExecuteAsync(collections, hexPos, caster, allocator, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return default;

            var accessMap = FormHexAccessMapCommand.Execute(resultingData.AsReadOnly(), hexPos, caster.TrianglesPerHexEdge);
            return new HexFlowMap(resultingData, accessMap);
        }

        public static HexFlowMap ExecuteWithCachedCollections(
            Allocator allocator,
            NavigationHexPosition hexPos,
            INavigationCaster caster,
            FlowFieldCalculationCollections collections)
        {
            caster.CastHex(hexPos, s_flowMapQueryParameters);
            //var refinedData = RefineNavRaycastDataCommand.Execute(hexPos, caster.Results, LOCK_PERCENT, caster);
            NativeHashMap<IntTriangularPos, TriangleNavData> refinedData = default;
            var data = collections.SetupData;

            foreach (var triangleKvp in refinedData)
            {
                var navdata = triangleKvp.Value;
                data.Set(triangleKvp.Key, navdata);
            }

            var resultingData = CombineFlowMapsCommand.Execute(collections, hexPos, caster, allocator);
            var accessMap = FormHexAccessMapCommand.Execute(resultingData.AsReadOnly(), hexPos, caster.TrianglesPerHexEdge);
            return new HexFlowMap(resultingData, accessMap);
        }

       
    }
}
