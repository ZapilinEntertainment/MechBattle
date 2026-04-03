using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct PrepareHexRaycastCommandsJob : IJob
    {
        public float2 HexCenterWorld;
        public float TriangleHeight;
        public int TrianglesPerHexEdge;
        public int RaycastTrianglesPerEdge;
        public float CastingHeight;
        public float CastingRayLength;
        public QueryParameters QueryParameters;


        public NativeArray<IntTriangularPos> Positions;
        public NativeArray<SubdivideTriangleIntoSmallerOnesCommand.SmallTriangleData> RaycastPoints;
        [WriteOnly] public NativeArray<RaycastCommand> RaycastCommands;

        public void Execute()
        {
            // note: all static functions inside are burstable
            var innerCircleTopTriangle = NavigationMapHelper.GetInnerCircleTopTriangle(HexCenterWorld, TriangleHeight);
            GetTrianglesInHexCommand.Execute(innerCircleTopTriangle, TrianglesPerHexEdge, Positions);

            // why Vector3: raycast command constructor use it
            var direction = Vector3.down;

            var subdivisionProtocol = new SubdivideTriangleIntoSmallerOnesCommand.TriangleSubdivisionProtocol()
            {
                Centers = RaycastPoints,
                TriangleHeight = TriangleHeight,
                RaycastTrianglesPerEdge = RaycastTrianglesPerEdge
            };

            var index = 0;
            foreach (var position in Positions)
            {
                var cartesian = TriangularMath.TriangularToWorld(position, TriangleHeight);
                SubdivideTriangleIntoSmallerOnesCommand.Execute(
                    cartesian.xz, 
                    position.IsPeak, 
                    subdivisionProtocol);

                var centers = subdivisionProtocol.Centers;
                for (var i = 0; i < RaycastPoints.Length; i++)
                {
                    var raycastPos = RaycastPoints[i].WorldPos;
                    RaycastCommands[index++] = new(new Vector3(raycastPos.x, CastingHeight, raycastPos.y), direction, QueryParameters, CastingRayLength);
                }
            }
        }
    }
}
