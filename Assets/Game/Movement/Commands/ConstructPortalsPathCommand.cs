using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public static class ConstructPortalsPathCommand
    {
        private struct PortalOption
        {
            public int PortalId;
            public float MinDist;
        }

        public static async Awaitable Execute(
            HexPathSearchRequest request,
            INavigationMap map, 
            HexDataAccessHandler hexDataAccessHandler,
            AwaitingTokensList awaitingTokesList)
        {
            // 1. check start hex existence
            var startHexCoord = request.StartHexCoord;

            if (!hexDataAccessHandler.TryGetHexData(startHexCoord, out var startHex, out var awaitingToken))
            {
                do
                {
                    await Task.Yield();
                }
                while (awaitingTokesList.IsTokenActive(awaitingToken));
            }

            // 2. calculate start hex distances map
            using var distanceCalculationProcess = new GeneratePointDistancesProcess(Allocator.TempJob, map);
            var distanceCalculationHandle = distanceCalculationProcess.Schedule(startHexCoord, request.StartTripos);
            do
            {
                await Task.Yield();
            }
            while (!distanceCalculationHandle.IsCompleted);
            var distancesData = new Dictionary<IntTriangularPos, float>();
            distanceCalculationProcess.UnloadDistanceDataInto(distancesData);


            // 3. get all accessible portals, write also shortest distance
            var portalsList = new List<PortalOption>();
            var directionCoefficients = HexTransitionLogic.GetDirectionCostCoefficients(startHexCoord, request.EndHexCoord);

            foreach (var portal in startHex.PortalsList)
            {
                var enumerator = portal.Edge.GetEdgeEnumerable(portal.Length, portal.StartTriangle);

                var minDist = float.MaxValue;
                var portalCf = directionCoefficients[portal.Edge];
                foreach (var portalTriangle in enumerator)
                {
                    minDist = math.min(minDist, distancesData[portalTriangle] * portalCf);
                }
                if (minDist == float.MaxValue)
                    continue;

                portalsList.Add(new() { MinDist = minDist, PortalId = portal.Id });
            }

            //  3.5 sort portals from closest to farthest, (reversed)
            if (portalsList.Count == 0)
                throw new System.NotImplementedException("start hex has no portals");
            portalsList.Sort((optionA, optionB) => optionB.MinDist.CompareTo(optionA.MinDist)); 

            //4. search shortest portals path to target 
            for (var i = 0; i < portalsList.Count; i++) 
            {
                // todo: do transitions caching
            }
        }
    
    }
}
