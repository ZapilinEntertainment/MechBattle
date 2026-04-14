using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class PrepareNavigationMapCommand
    {
        public static NavigationMap Execute(in MapSettings settings, INavigationCaster caster)
        {
            var map = new NavigationMap(settings);
            CastMap(map, caster);
            return map;
        }

        public static void CastMap(INavigationMap map, INavigationCaster caster) 
        {
            var allocator = Allocator.TempJob;
            var settings = map.Settings;
            using var hexes = GetHexesInRectangleCommand.Execute(settings, allocator);

            using var walkableSurfaceCaster = new NavigationCaster(settings, allocator);
            using var obstaclesCaster = new NavigationCaster(settings, allocator);         
            var walkableQueryParameters = NavigationConstants.GetWalkableCastQueryParameters();
            var obstacleQueryParameters = NavigationConstants.GetObstacleCastQueryParameters();

            for (var i = 0; i < hexes.Length; i++)
            {
                var hexCoord = hexes[i];
                var hexPos = new NavigationHexPosition(hexCoord, map.HexEdgeSize, map.TrianglesPerHexEdge);

                var walkableDataHandle = walkableSurfaceCaster.PrepareCastJob(hexPos, walkableQueryParameters);
                var obstacleDataHandle = obstaclesCaster.PrepareCastJob(hexPos, obstacleQueryParameters);

                walkableDataHandle.Complete();
                obstacleDataHandle.Complete();



                //raycastJobCollections.ChangeHexPosAndReset(hexPos.TriangularCenterPos);

                //using var walkableData = caster.CastHex(Allocator.TempJob, hexPos.CenterPosWorld, NavigationConstants.GetWalkableCastQueryParameters());
                //var refinedData = RefineNavRaycastDataCommand.Execute(hexPos, raycastData.AsReadOnly(), LOCK_PERCENT, caster);
                //var data = collections.SetupData;

                //foreach (var triangleKvp in refinedData)
                //{
                //    var navdata = triangleKvp.Value;
                //    data.Set(triangleKvp.Key, navdata);
                //}

                //var resultingData = CombineFlowMapsCommand.Execute(collections, hexPos, caster, allocator);
                //var accessMap = FormHexAccessMapCommand.Execute(resultingData.AsReadOnly(), hexPos, caster.TrianglesPerHexEdge);
                //var flowMap =  new HexFlowMap(resultingData, accessMap);

                //map.UpdateHexFlowMap(hexCoord, flowMap);
            }
        }
    
    }
}
