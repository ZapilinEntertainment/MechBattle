using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class CalculateHexPortalsCommand
    {
        private struct EdgeEnumerationProtocol
        {
            public readonly INavigationMap Map;
            public readonly int TrianglesPerEdge;
            public readonly NavigationHexPosition HexPos;
            public HexEdge Edge;
           

            public EdgeEnumerationProtocol(INavigationMap map, int2 hexCoord)
            {
                Map = map;
                TrianglesPerEdge = map.TrianglesPerHexEdge;
                HexPos = new NavigationHexPosition(hexCoord, map);
                Edge = HexEdge.Top;
            }
        }

        public static List<NavigationPortalExit> CalculateExitsList(INavigationMap map, int2 hexCoord, HexEdge edge)
        {        
            var protocol = new EdgeEnumerationProtocol(map, hexCoord) { Edge = edge};
            switch (edge)
            {
                case HexEdge.TopRight: return FindPortalExits<TopRightEdgeEnumerationLogic>(protocol);
                case HexEdge.BottomRight: return FindPortalExits<BottomRightEdgeEnumerationLogic>(protocol);
                case HexEdge.Bottom: return FindPortalExits<BottomEdgeEnumerationLogic>(protocol);
                case HexEdge.BottomLeft: return FindPortalExits<BottomLeftEdgeEnumerationLogic>(protocol);
                case HexEdge.TopLeft: return FindPortalExits<TopLeftEdgeEnumerationLogic>(protocol);
                default: return FindPortalExits<TopEdgeEnumerationLogic>(protocol);
            }
        }

        private static List<NavigationPortalExit> FindPortalExits<T>(in EdgeEnumerationProtocol protocol) where T : struct, IEdgeEnumerationLogic
        {
            var enumerator = new EdgeEnumerator<T>(protocol.TrianglesPerEdge, protocol.HexPos);
            var startTripos = enumerator.Current;
            var hexCoord = protocol.HexPos.HexCoordinate;
            var list = new List<NavigationPortalExit>();

            var edge = protocol.Edge;
            var peakDir = (int)edge.ToNeighbourDirectionFromPeak();
            var valleyDir = (int)edge.ToNeighbourDirectionFromValley();
            var trianglesPassed = 0;
            var currentZoneIndex = 0;
            var currentNeighbourZoneIndex = 0;
            var sequenceStarted = false;

            void StartNewSequence(IntTriangularPos tripos, int zoneIndex, int neighbourZoneIndex)
            {
                startTripos = tripos;
                trianglesPassed = 1;
                currentZoneIndex = zoneIndex;
                currentNeighbourZoneIndex = neighbourZoneIndex;
                sequenceStarted = true;
            }

            void FinishSequence()
            {
                if (trianglesPassed == 0)
                    return;
                list.Add(new(startTripos, edge, trianglesPassed, currentZoneIndex));
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
                    if (!passableData.IsNeighbourAccessible(direction))
                    {
                        FinishSequence();
                    }
                    else
                    {
                        var neighbourData = map.GetPassabilityData(TriangularMath.GetNeighbourByDirection(tripos, direction));
                        if (sequenceStarted)
                        {
                            if (neighbourData.ZoneIndex != currentNeighbourZoneIndex)
                            {
                                FinishSequence();
                            }
                            else
                            {
                                trianglesPassed++;
                            }
                        }
                        else
                        {
                            StartNewSequence(tripos, passableData.ZoneIndex, neighbourData.ZoneIndex);
                        }
                    }
                }
            }

            return list;
        }
    
    }
}
