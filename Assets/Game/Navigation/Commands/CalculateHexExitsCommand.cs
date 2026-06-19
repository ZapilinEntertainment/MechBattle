using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class CalculateHexExitsCommand
    {
        private struct EdgeEnumerationProtocol
        {
            public readonly INavigationMap Map;
            public readonly int TrianglesPerEdge;
            public readonly NavigationHexPosition HexPos;
            public readonly ICollection<NavigationPortalExit> ExitsList;
            public HexEdge Edge;
           

            public EdgeEnumerationProtocol(INavigationMap map, int2 hexCoord, ICollection<NavigationPortalExit> exitsList)
            {
                Map = map;
                TrianglesPerEdge = map.TrianglesPerHexEdge;
                HexPos = new NavigationHexPosition(hexCoord, map);
                ExitsList = exitsList;
                Edge = HexEdge.Top;
            }
        }

        public static void Execute(INavigationMap map, int2 hexCoord, HexEdge edge, ICollection<NavigationPortalExit> exitsList)
        {        
            var protocol = new EdgeEnumerationProtocol(map, hexCoord, exitsList) { Edge = edge};
            switch (edge)
            {
                case HexEdge.TopRight: FindPortalExits<TopRightEdgeEnumerationLogic>(protocol); break;
                case HexEdge.BottomRight: FindPortalExits<BottomRightEdgeEnumerationLogic>(protocol); break;
                case HexEdge.Bottom: FindPortalExits<BottomEdgeEnumerationLogic>(protocol); break;
                case HexEdge.BottomLeft: FindPortalExits<BottomLeftEdgeEnumerationLogic>(protocol); break;
                case HexEdge.TopLeft: FindPortalExits<TopLeftEdgeEnumerationLogic>(protocol); break;
                default: FindPortalExits<TopEdgeEnumerationLogic>(protocol); break;
            }
        }

        private static void FindPortalExits<T>(in EdgeEnumerationProtocol protocol) where T : struct, IEdgeEnumerationLogic
        {
            var enumerator = new EdgeEnumerator<T>(protocol.TrianglesPerEdge, protocol.HexPos);
            var startTripos = enumerator.Current;

            var edge = protocol.Edge;
            var peakDir = (int)edge.ToNeighbourDirectionFromPeak();
            var valleyDir = (int)edge.ToNeighbourDirectionFromValley();
            var trianglesPassed = 0;
            var currentZoneIndex = 0;
            var currentNeighbourZoneIndex = 0;
            var sequenceStarted = false;
            var list = protocol.ExitsList;
            var startIndex = 0;
            var index = 0;

            void StartNewSequence(IntTriangularPos tripos, int zoneIndex, int neighbourZoneIndex)
            {
                startTripos = tripos;
                trianglesPassed = 1;
                currentZoneIndex = zoneIndex;
                currentNeighbourZoneIndex = neighbourZoneIndex;
                sequenceStarted = true;
                startIndex = index;
            }          

            void FinishSequence()
            {
                if (trianglesPassed == 0)
                    return;
                list.Add(new(startTripos, startIndex, edge, trianglesPassed, currentZoneIndex));
                trianglesPassed = 0;
                sequenceStarted = false;
            }

            var map = protocol.Map;
            
            foreach (var tripos in enumerator)
            {
                var passableData = map.GetPassabilityData(tripos);                
                if (!passableData.IsPassable)
                {                    
                    FinishSequence();
                }
                else
                {
                    var direction = tripos.IsPeak ? peakDir : valleyDir;
                    //UnityEngine.Debug.Log($"{tripos} -> {passableData.IsNeighbourAccessible(direction)} ({direction})");
                    if (!passableData.IsNeighbourAccessible(direction))
                    {
                        FinishSequence();
                    }
                    else
                    {
                        var neighbourPos = TriangularMath.GetNeighbourByDirection(tripos, direction);
                        var neighbourData = map.GetPassabilityData(neighbourPos);
                        var cellZone = passableData.ZoneIndex;
                        var neighbourCellZone = neighbourData.ZoneIndex;

                        if (sequenceStarted)
                        {
                            if (neighbourCellZone == currentNeighbourZoneIndex && cellZone == currentZoneIndex)
                            {
                                trianglesPassed++;
                            }
                            else
                            {
                                FinishSequence();
                                StartNewSequence(tripos, cellZone, neighbourCellZone);
                            }
                        }
                        else
                        {                            
                            StartNewSequence(tripos, cellZone, neighbourCellZone);                            
                        }
                    }
                }
                index++;
            }

            if (sequenceStarted)
                FinishSequence();
        }
    
    }
}
