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
        public float TriangleHeight;
        public int TrianglesPerEdge;
        public int RaycastTrianglesPerEdge;
        public float CastingHeight;
        public float CastingRayLength;
        public NavigationHexPosition HexPos;
        public QueryParameters QueryParameters;

        public NativeArray<SubdivideTriangleCommand.SmallTriangleData> RaycastPoints;
        [WriteOnly] public NativeArray<RaycastCommand> RaycastCommands;

        public void Execute()
        {
            // why Vector3: raycast command constructor use it
            var direction = Vector3.down;

            var subdivisionProtocol = new SubdivideTriangleCommand.TriangleSubdivisionProtocol()
            {
                Centers = RaycastPoints,
                TriangleHeight = TriangleHeight,
                RaycastTrianglesPerEdge = RaycastTrianglesPerEdge
            };

            var index = 0;
            foreach (var tripos in new HexTrianglesEnumerator(HexPos.TriangularCenterPos, TrianglesPerEdge))
            {
                SubdivideTriangleCommand.Execute(
                    tripos, 
                    subdivisionProtocol);

                var centers = subdivisionProtocol.Centers;
                for (var i = 0; i < RaycastPoints.Length; i++)
                {
                    var subtrianglePos = RaycastPoints[i].WorldPos;
                    var raycastPos = new Vector3(subtrianglePos.x, CastingHeight, subtrianglePos.y);
                    RaycastCommands[index++] = new(raycastPos, direction, QueryParameters, CastingRayLength);
                }

               
            }
        }
    }
}
