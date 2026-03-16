using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    public static class HexMath
    {

        // chatGPT generated
        [BurstCompile]
        public static int CalculateDistance(int2 hexPosA, int2 hexPosB)
        {
            int x1 = hexPosA.x;
            int z1 = hexPosA.y;
            int y1 = -x1 - z1;

            int x2 = hexPosB.x;
            int z2 = hexPosB.y;
            int y2 = -x2 - z2;

            return math.max(
                math.abs(x1 - x2),
                math.max(
                    math.abs(y1 - y2),
                    math.abs(z1 - z2)
                )
            );
        }

        [BurstCompile]
        public static float CalculateDistance(float2 hexPosA, float2 hexPosB)
        {
            var x1 = hexPosA.x;
            var z1 = hexPosA.y;
            var y1 = -x1 - z1;

            var x2 = hexPosB.x;
            var z2 = hexPosB.y;
            var y2 = -x2 - z2;

            return math.max(
                math.abs(x1 - x2),
                math.max(
                    math.abs(y1 - y2),
                    math.abs(z1 - z2)
                )
            );
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float CalculateDistance(HexPathNodeKey A, HexPathNodeKey B) => CalculateDistance(A.EdgeCenterHexCoord, B.EdgeCenterHexCoord);

        //chatgpt generated
        [BurstCompile]
        public static int2 DefineHex(float2 worldPos, float hexEdge)
        {
            var q = (2f / 3f * worldPos.x) / hexEdge;
            var r = (-1f / 3f * worldPos.x + NavigationConstants.SQRT_THREE_D_3 * worldPos.y) / hexEdge;

            return AxialRound(q, r);
        }

        [BurstCompile]
        public static float2 WorldToHex(float2 worldPos, float hexEdge)
        {
            var q = (2f / 3f * worldPos.x) / hexEdge;
            var r = (-1f / 3f * worldPos.x + NavigationConstants.SQRT_THREE_D_3 * worldPos.y) / hexEdge;

            return new(q, r);
        }

        //chatgpt generated
        [BurstCompile]
        public static float2 HexToWorld(int2 pos, float hexEdge)
        {
            var x = hexEdge * (3f / 2f * pos.x);
            var y = hexEdge * (NavigationConstants.SQRT_OF_THREE * (pos.y + pos.x / 2f));
            return new float2(x, y);
        }

        [BurstCompile]
        public static float2 HexToWorld(float2 pos, float hexEdge)
        {
            var x = hexEdge * (3f / 2f * pos.x);
            var y = hexEdge * (NavigationConstants.SQRT_OF_THREE * (pos.y + pos.x / 2f));
            return new float2(x, y);
        }

        //chatgpt generated
        [BurstCompile]
        private static int2 AxialRound(float q, float r)
        {
            var x = q;
            var z = r;
            var y = -x - z;

            var rx = (int)math.round(x);
            var ry = (int)math.round(y);
            var rz = (int)math.round(z);

            var dx = math.abs(rx - x);
            var dy = math.abs(ry - y);
            var dz = math.abs(rz - z);

            if (dx > dy && dx > dz)
                rx = -ry - rz;
            else if (dy > dz)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            return new int2(rx, rz);
        }

    }
}
