using System;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class SubdivideTriangleCommand
    {
        private const float ORTHOCENTER_CF = 2f * NavigationConstants.DIV_THREE; 

        public readonly struct SmallTriangleData
        {
            public readonly float2 WorldPos;
            public readonly bool IsPeak;
            public float3 WorldPosV3 => new float3(WorldPos.x, 0f, WorldPos.y);

            public SmallTriangleData(float2 pos, bool isPeak)
            {
                WorldPos = pos;
                IsPeak = isPeak;
            }
        }

        // why protocol and no collection creation: multiple re-using in scenarios
        public struct TriangleSubdivisionProtocol
        {
            public float TriangleHeight;
            public int RaycastTrianglesPerEdge;

            public float SubdividedTriangleHeight => TriangleHeight / RaycastTrianglesPerEdge;

            public NativeArray<SmallTriangleData> Centers;
        }

        public static NativeArray<SmallTriangleData> CreateDataArray(int trianglesPerEdge, Allocator allocator) => 
            new NativeArray<SmallTriangleData>(trianglesPerEdge * trianglesPerEdge, allocator, NativeArrayOptions.UninitializedMemory);


        public static void Execute(
             IntTriangularPos pos,
             TriangleSubdivisionProtocol protocol)
        {
            var subdivisions = protocol.RaycastTrianglesPerEdge;
            var localPinnaclePos = pos.IsPeak ? HexSector.Bottom.GetPinnaclePos(new (0, -1, 0), subdivisions) : HexSector.Top.GetPinnaclePos(new(0, 1, 0), subdivisions);

            var offset = new float2(0f, ORTHOCENTER_CF * protocol.TriangleHeight * (pos.IsPeak ? 1 : -1));
            var triangleCenter = TriangularMath.TriangularToWorld(pos, protocol.TriangleHeight);
            var zeroPos = triangleCenter.xz + offset;

            var i = 0;
            var subdividedHeight = protocol.SubdividedTriangleHeight;
            foreach (var subPos in new SubtrianglesCoordsEnumerator(localPinnaclePos, subdivisions))
            {
                protocol.Centers[i++] = new(TriangularMath.TriangularToWorld(subPos, subdividedHeight).xz + zeroPos, subPos.IsPeak);
            }
        }
    }
}
