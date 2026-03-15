using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class CalculateHexFlowMapCommand
    {
        private static readonly QueryParameters s_flowMapQueryParameters = NavigationConstants.GetGroundCastQueryParameters();
        private const float LOCK_PERCENT = NavigationConstants.NAV_OBSTACLES_LOCK_PERCENT;

        public static async Awaitable<HexFlowMap> ExecuteAsync(NavigationHexPosition hex, INavigationCaster caster, CancellationToken cancellationToken)
        {
            var raycastData = await caster.CastHexAsync(hex.CenterPos, s_flowMapQueryParameters, cancellationToken);
            if (cancellationToken.IsCancellationRequested)
            {
                raycastData.Dispose();
                return default;
            }

            // TODO: move casting to own command
            var refinedData = RefineNavRaycastDataCommand.Execute(raycastData.AsReadOnly(), LOCK_PERCENT, caster);
            foreach (var triangleKvp in refinedData)
            {

            }

            var trianglesInHexCount = caster.HexTrianglesCount;
            var data = new SquaredHexTrianglesList<FlowFieldCellData>(hex.TriangularCenterPos, caster.TrianglesPerHexEdge, Allocator.TempJob);
            var job = new GenerateFlowFieldJob()
            {
                Data = data                
            };

            // TODO: complete, calculate every of 6 edges and combine them
            return null;
        }
    
    }
}
