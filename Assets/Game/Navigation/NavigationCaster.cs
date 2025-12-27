using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class NavigationCaster
    {

        public struct HexRaycastProtocol
        {
            public int HexTrianglesPerEdge;
            public int RaycastTrianglesPerEdge;
            public float TriangleEdgeSize;
            public float CastingHeight;
            public float CastingRayLength;
            public QueryParameters QueryParameters;

            // temporal data (provide for re-using)
            public NativeArray<IntTriangularPos> TempPositionsArray;
            public NativeArray<float2> TempRaycastPointsArray;

            // final commands list
            public NativeList<RaycastCommand> RaycastCommands;
        }

        [BurstCompile]
        public static void PrepareRaycastCommands(float2 hexCenter, in HexRaycastProtocol protocol)
        {            
            var innerCircleTopTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(hexCenter, protocol.TriangleEdgeSize);
            var positions = protocol.TempPositionsArray;
            NavigationMapHelper.GetTrianglesInHex(innerCircleTopTriangle, protocol.HexTrianglesPerEdge, positions);

            var commands = protocol.RaycastCommands;
            var direction = Vector3.down;

            var subdivisionProtocol = new NavigationMapHelper.TriangleSubdivisionProtocol()
            {
                Centers = protocol.TempRaycastPointsArray,
                TriangleEdgeLength = protocol.TriangleEdgeSize,
                RaycastTrianglesPerEdge = protocol.RaycastTrianglesPerEdge                
            };

            foreach (var position in positions)
            {
                var cartesian = TriangularMath.TriangularToWorld(position, protocol.TriangleEdgeSize);
                var raycastPoints = protocol.TempRaycastPointsArray;
                NavigationMapHelper.SubdivideTriangleIntoSmallerAndGetCenters(cartesian.xz, position.IsPeak, subdivisionProtocol);
                foreach (var raycastPos in subdivisionProtocol.Centers)
                {
                    commands.Add(new(new Vector3(raycastPos.x, protocol.CastingHeight, raycastPos.y), direction, protocol.QueryParameters, protocol.CastingRayLength));
                }
            }
        }
    }
}
