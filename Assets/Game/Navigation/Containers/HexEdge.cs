using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace ZE.MechBattle.Navigation
{
    public enum HexEdge : byte { Top, TopRight, BottomRight, Bottom, BottomLeft, TopLeft }

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
                case HexEdge.TopRight: return new(1, 0);
                case HexEdge.BottomRight: return new(1, -1);
                case HexEdge.Bottom: return new(0, -1);
                case HexEdge.BottomLeft: return new(-1, 0);
                case HexEdge.TopLeft: return new(-1, 1);
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
                case HexEdge.TopRight: return PeakNeighbour.EdgeUpRight;
                case HexEdge.BottomRight: return PeakNeighbour.VertexDownRightValley;
                case HexEdge.Bottom: return PeakNeighbour.EdgeDown;
                case HexEdge.BottomLeft: return PeakNeighbour.VertexDownLeftValley;
                case HexEdge.TopLeft: return PeakNeighbour.EdgeUpLeft;
                default: return PeakNeighbour.VertexUp;
            }
        }

        [BurstCompile]
        public static ValleyNeighbour ToNeighbourDirectionFromValley(this HexEdge edge)
        {
            switch (edge)
            {
                case HexEdge.TopRight: return ValleyNeighbour.VertexUpRightPeak;
                case HexEdge.BottomRight: return ValleyNeighbour.EdgeDownRight;
                case HexEdge.Bottom: return ValleyNeighbour.VertexDown;
                case HexEdge.BottomLeft: return ValleyNeighbour.EdgeDownLeft;
                case HexEdge.TopLeft: return ValleyNeighbour.VertexUpLeftPeak;
                default: return ValleyNeighbour.EdgeUp;
            }
        }
    }
}
