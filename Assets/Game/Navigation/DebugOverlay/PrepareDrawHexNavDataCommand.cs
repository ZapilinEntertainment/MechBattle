using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation.DebugOverlay
{
    // showing cast points and returns locked triangles (standartized coords)
    internal static class PrepareDrawHexNavDataCommand
    {

        internal static async Awaitable ExecuteAsync(
            float2 hexCenter, 
            INavigationCaster caster,
            float intersectionPercentForLock,
            List<SphereDrawData> drawData,
            CancellationToken token)
        {
            var queryParameters = NavigationConstants.GetGroundCastQueryParameters();
            var raycastResults = await caster.CastHexAsync(hexCenter,queryParameters, token);
            if (token.IsCancellationRequested)
                return;

            var refinedData = RefineNavRaycastDataCommand.Execute(raycastResults.AsReadOnly(), intersectionPercentForLock, caster);
            foreach (var raycastResult in raycastResults)
            {
                var pos = raycastResult.point;
                var trianglePos = TriangularMath.WorldToTrianglePos(pos, caster.TriangleHeight);
                if (refinedData.TryGetValue(trianglePos, out var data))
                {
                    drawData.Add(new(pos, data.IsPassable ? DebugColor.Green : DebugColor.Red, 0.5f));
                }
                else
                {
                    drawData.Add(new(pos, DebugColor.Black, 0.5f));
                }
            }
            raycastResults.Dispose();
        }

    }
}
