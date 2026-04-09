using System.Threading;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public static class CalculateHexFlowMapCommand
    {
        private static readonly QueryParameters s_flowMapQueryParameters = NavigationConstants.GetGroundCastQueryParameters();
        private const float LOCK_PERCENT = NavigationConstants.NAV_OBSTACLES_LOCK_PERCENT;

        public static FlowFieldCalculationCollections PrepareCalculationCollections(Allocator allocator, NavigationHexPosition hexPos, INavigationCaster caster) =>
            new FlowFieldCalculationCollections(allocator, hexPos.TriangularCenterPos, caster.TrianglesPerHexEdge);

        public static async Awaitable<HexFlowMap> ExecuteAsync(
            Allocator allocator,
            NavigationHexPosition hex, 
            INavigationCaster caster, 
            CancellationToken cancellationToken)
        {
            using var collections = PrepareCalculationCollections(Allocator.TempJob, hex, caster);   
            return await ExecuteAsyncWithCachedCollections(allocator, hex, caster, collections, cancellationToken);         
        }

        public static async Awaitable<HexFlowMap> ExecuteAsyncWithCachedCollections(
            Allocator allocator,
            NavigationHexPosition hexPos,
            INavigationCaster caster,
            FlowFieldCalculationCollections collections,
            CancellationToken cancellationToken)
        {
            using var raycastData = await caster.CastHexAsync(Allocator.TempJob, hexPos.CenterPosWorld, s_flowMapQueryParameters, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                return default;
            }

            // TODO: move casting to own command
            var refinedData = RefineNavRaycastDataCommand.Execute(hexPos, raycastData.AsReadOnly(), LOCK_PERCENT, caster);
            var data = collections.SetupData;

            foreach (var triangleKvp in refinedData)
            {
                var navdata = triangleKvp.Value;
                data.Set(triangleKvp.Key, navdata);
            }

            #if UNITY_EDITOR
            if (FlowMapCellData.STRUCTURE_SIZE * 6 * data.Length > 1024 * 900)
                throw new System.Exception("potential stack overflow");
            #endif


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
            using var raycastData = caster.CastHex(Allocator.TempJob, hexPos.CenterPosWorld, s_flowMapQueryParameters);
            var refinedData = RefineNavRaycastDataCommand.Execute(hexPos, raycastData.AsReadOnly(), LOCK_PERCENT, caster);
            var data = collections.SetupData;

            foreach (var triangleKvp in refinedData)
            {
                var navdata = triangleKvp.Value;
                data.Set(triangleKvp.Key, navdata);
            }

#if UNITY_EDITOR
            if (FlowMapCellData.STRUCTURE_SIZE * 6 * data.Length > 1024 * 900)
                throw new System.Exception("potential stack overflow");
#endif

            var resultingData = CombineFlowMapsCommand.Execute(collections, hexPos, caster, allocator);
            var accessMap = FormHexAccessMapCommand.Execute(resultingData.AsReadOnly(), hexPos, caster.TrianglesPerHexEdge);
            return new HexFlowMap(resultingData, accessMap);
        }

       
    }
}
