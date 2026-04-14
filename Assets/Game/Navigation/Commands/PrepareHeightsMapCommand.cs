using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class PrepareHeightsMapCommand
    {
        public static void Execute(INavigationMap map, NavigationCaster caster)
        {
            //using var raycastJobCollections = CalculateHexFlowMapCommand.PrepareCalculationCollections(Allocator.TempJob, default, caster);
            //var hexCoords = map.HexCoords;
            //for (var i = 0; i < hexCoords.Length; i++)
            //{
            //    var hexCoord = hexCoords[i];
            //    var hexPos = new NavigationHexPosition(hexCoord, _map.HexEdgeSize, _map.TrianglesPerHexEdge);
            //    raycastJobCollections.ChangeHexPosAndReset(hexPos.TriangularCenterPos);

            //    var flowMap = CalculateHexFlowMapCommand.ExecuteWithCachedCollections(
            //        allocator,
            //        hexPos,
            //        caster,
            //        raycastJobCollections);

            //    _map.UpdateHexFlowMap(hexCoord, flowMap);
            //    _trisDrawer.DrawHexTriangles(hexPos, _map);
            //}
        }
    
    }
}
