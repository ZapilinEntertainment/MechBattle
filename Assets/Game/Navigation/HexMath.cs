using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

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

    }
}
