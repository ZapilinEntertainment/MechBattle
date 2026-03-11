using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public enum HexEdge : byte { Up, UpRight, DownRight, Down, DownLeft, UpLeft }

    public static class HexEdgeExtension
    {
        [BurstCompile]
        public static HexEdge ToOpposite(this HexEdge edge) => (HexEdge)(((int)edge + 3) % 6);

        [BurstCompile]
        public static int2 ToOffsetVector(this HexEdge edge)
        {
            switch (edge)
            {
                case HexEdge.UpRight: return new(1, 1);
                case HexEdge.DownRight: return new(1, -1);
                case HexEdge.Down: return new(0, -1);
                case HexEdge.DownLeft: return new(-1, -1);
                case HexEdge.UpLeft: return new(-1, 1);
                default: return new(0, 1);
            }
        }
    }
}
