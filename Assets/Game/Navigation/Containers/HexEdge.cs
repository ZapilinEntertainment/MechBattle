using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    public enum HexEdge : byte { Up, UpRight, DownRight, Down, DownLeft, UpLeft }

    public static class HexEdgeExtension
    {
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HexEdge ToOpposite(this HexEdge edge) => (HexEdge)(((int)edge + 3) % 6);

        [BurstCompile]
        public static int2 ToHexOffsetVector(this HexEdge edge)
        {
            switch (edge)
            {
                case HexEdge.UpRight: return new(1, 0);
                case HexEdge.DownRight: return new(1, -1);
                case HexEdge.Down: return new(0, -1);
                case HexEdge.DownLeft: return new(-1, 0);
                case HexEdge.UpLeft: return new(-1, 1);
                default: return new(0, 1);
            }
        }

        [BurstCompile]
        public static float2 ToEdgePosOffsetVector(this HexEdge edge)
        {
            var offsetVector = (float2)edge.ToHexOffsetVector();
            return 0.5f * offsetVector;
        }

        [BurstCompile]
        public static PeakNeighbour ToNeighbourDirectionFromPeak(this HexEdge edge)
        {
            switch (edge)
            {
                case HexEdge.UpRight: return PeakNeighbour.EdgeUpRight;
                case HexEdge.DownRight: return PeakNeighbour.VertexDownRightValley;
                case HexEdge.Down: return PeakNeighbour.EdgeDown;
                case HexEdge.DownLeft: return PeakNeighbour.VertexDownLeftValley;
                case HexEdge.UpLeft: return PeakNeighbour.EdgeUpLeft;
                default: return PeakNeighbour.VertexUp;
            }
        }

        [BurstCompile]
        public static ValleyNeighbour ToNeighbourDirectionFromValley(this HexEdge edge)
        {
            switch (edge)
            {
                case HexEdge.UpRight: return ValleyNeighbour.VertexUpRightPeak;
                case HexEdge.DownRight: return ValleyNeighbour.EdgeDownRight;
                case HexEdge.Down: return ValleyNeighbour.VertexDown;
                case HexEdge.DownLeft: return ValleyNeighbour.EdgeDownLeft;
                case HexEdge.UpLeft: return ValleyNeighbour.VertexUpLeftPeak;
                default: return ValleyNeighbour.EdgeUp;
            }
        }
    }
}
