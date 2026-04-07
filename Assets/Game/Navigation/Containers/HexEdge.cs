using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

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

        [BurstCompile]
        public static int3 GetHexCornerOffsetTriangularVector(this HexEdge edge, bool clockwise)
        {
            switch ((edge, clockwise))
            {
                // top, clockwise = top-right counter-clockwise
                case (HexEdge.Top, true):
                case (HexEdge.TopRight, false):
                    return new int3(-1, 1, 0);

                // top-right cw = bottom-right cc
                case (HexEdge.TopRight, true):
                case (HexEdge.BottomRight, false):
                    return new int3(-1, 0, 1);

                // bottom-right cw = bottom cc
                case (HexEdge.BottomRight, true):
                case (HexEdge.Bottom, false):
                    return new int3(0, -1, 1);

                // bottom cw = bottom-left cc
                case (HexEdge.Bottom, true):
                case (HexEdge.BottomLeft, false):
                    return new(1, -1, 0);

                // bottom-left cw = top-left cc
                case (HexEdge.BottomLeft, true):
                case (HexEdge.TopLeft, false):
                    return new(1, 0, -1);

                // top cc = top-left cw
                case (HexEdge.TopLeft, true):
                default:
                    return new int3(0, 1, -1);
            }
        }


        // TEST PURPOSES ONLY
        [BurstDiscard]
        public static IEnumerable<IntTriangularPos> GetEdgeEnumerable(this HexEdge edge, int trianglesPerEdge, NavigationHexPosition hexPos)
        {
            switch (edge)
            {
                case HexEdge.TopRight: return new EdgeEnumerator<TopRightEdgeEnumerationLogic>(trianglesPerEdge, hexPos);
                case HexEdge.BottomRight: return new EdgeEnumerator<BottomRightEdgeEnumerationLogic>(trianglesPerEdge, hexPos);
                case HexEdge.Bottom: return new EdgeEnumerator<BottomEdgeEnumerationLogic>(trianglesPerEdge, hexPos);
                case HexEdge.BottomLeft: return new EdgeEnumerator<BottomLeftEdgeEnumerationLogic>(trianglesPerEdge, hexPos);
                case HexEdge.TopLeft: return new EdgeEnumerator<TopLeftEdgeEnumerationLogic>(trianglesPerEdge, hexPos);
                default: return new EdgeEnumerator<TopEdgeEnumerationLogic>(trianglesPerEdge, hexPos);
            }
        }

        [BurstCompile]
        public static BitField32 GetPeakTriangleEdgeNeighboursMask(this HexEdge edge)
        {
            var mask = new BitField32();
            switch(edge) 
            {
                case HexEdge.TopRight:
                    {
                        mask.SetBits((int)PeakNeighbour.VertexUp, true);
                        mask.SetBits((int)PeakNeighbour.VertexUpRight, true);
                        mask.SetBits((int)PeakNeighbour.EdgeUpRight, true);
                        mask.SetBits((int)PeakNeighbour.VertexRight, true);
                        mask.SetBits((int)PeakNeighbour.VertexDownRightValley, true);
                        break;
                    }
                    case HexEdge.BottomRight:
                    {
                        mask.SetBits((int)PeakNeighbour.VertexRight, true);
                        mask.SetBits((int)PeakNeighbour.VertexDownRightValley, true);
                        mask.SetBits((int)PeakNeighbour.VertexDownRightPeak, true);
                        break;
                    }
                    case HexEdge.Bottom:
                    {
                        mask.SetBits((int)PeakNeighbour.VertexDownRightValley, true);
                        mask.SetBits((int)PeakNeighbour.VertexDownRightPeak, true);
                        mask.SetBits((int)PeakNeighbour.EdgeDown, true);
                        mask.SetBits((int)PeakNeighbour.VertexDownLeftPeak, true);
                        mask.SetBits((int)PeakNeighbour.VertexDownLeftValley, true);
                        break;
                    }
                    case HexEdge.BottomLeft:
                    {
                        mask.SetBits((int)PeakNeighbour.VertexDownLeftPeak, true);
                        mask.SetBits((int)PeakNeighbour.VertexDownLeftValley, true);
                        mask.SetBits((int)PeakNeighbour.VertexLeft, true);
                        break;
                    }
                    case HexEdge.TopLeft:
                    {
                        mask.SetBits((int)PeakNeighbour.VertexDownLeftValley, true);
                        mask.SetBits((int)PeakNeighbour.VertexLeft, true);
                        mask.SetBits((int)PeakNeighbour.EdgeUpLeft, true);
                        mask.SetBits((int)PeakNeighbour.VertexUpLeft, true);
                        mask.SetBits((int)PeakNeighbour.VertexUp, true);
                        break;
                    }
                default:
                    {
                        mask.SetBits((int)PeakNeighbour.VertexLeft, true);
                        mask.SetBits((int)PeakNeighbour.VertexUp, true);
                        mask.SetBits((int)PeakNeighbour.VertexRight, true);
                        break;
                    }
            }
            return mask;
        }

        [BurstCompile]
        public static BitField32 GetValleyTriangleEdgeNeighboursMask(this HexEdge edge)
        {
            var mask = new BitField32();
            switch (edge)
            {
                case HexEdge.TopRight:
                    {
                        mask.SetBits((int)ValleyNeighbour.VertexUpRightValley, true);
                        mask.SetBits((int)ValleyNeighbour.VertexUpRightPeak, true);
                        mask.SetBits((int)ValleyNeighbour.VertexRight, true);
                        break;
                    }
                case HexEdge.BottomRight:
                    {
                        mask.SetBits((int)ValleyNeighbour.VertexUpRightPeak, true);
                        mask.SetBits((int)ValleyNeighbour.VertexRight, true);
                        mask.SetBits((int)ValleyNeighbour.EdgeDownRight, true);
                        mask.SetBits((int)ValleyNeighbour.VertexDownRight, true);
                        mask.SetBits((int)ValleyNeighbour.VertexDown, true);
                        break;
                    }
                case HexEdge.Bottom:
                    {
                        mask.SetBits((int)ValleyNeighbour.VertexDownRight, true);
                        mask.SetBits((int)ValleyNeighbour.VertexDown, true);
                        mask.SetBits((int)ValleyNeighbour.VertexDownLeft, true);
                        break;
                    }
                case HexEdge.BottomLeft:
                    {
                        mask.SetBits((int)ValleyNeighbour.VertexDown, true);
                        mask.SetBits((int)ValleyNeighbour.VertexDownLeft, true);
                        mask.SetBits((int)ValleyNeighbour.EdgeDownLeft, true);
                        mask.SetBits((int)ValleyNeighbour.VertexLeft, true);
                        mask.SetBits((int)ValleyNeighbour.VertexUpLeftPeak, true);
                        break;
                    }
                case HexEdge.TopLeft:
                    {
                        mask.SetBits((int)ValleyNeighbour.VertexLeft, true);
                        mask.SetBits((int)ValleyNeighbour.VertexUpLeftPeak, true);
                        mask.SetBits((int)ValleyNeighbour.VertexUpLeftValley, true);
                        break;
                    }
                default:
                    {
                        mask.SetBits((int)ValleyNeighbour.VertexUpLeftPeak, true);
                        mask.SetBits((int)ValleyNeighbour.VertexUpLeftValley, true);
                        mask.SetBits((int)ValleyNeighbour.EdgeUp, true);
                        mask.SetBits((int)ValleyNeighbour.VertexUpRightValley, true);
                        mask.SetBits((int)ValleyNeighbour.VertexUpRightPeak, true);
                        break;
                    }
            }
            return mask;
        }

        [BurstCompile]
        public static IntTriangularPos GetEdgeCenterPos(this HexEdge edge, IntTriangularPos hexCenter, int radius)
        {
            switch (edge)
            {
                case HexEdge.TopRight:
                    {
                        var valleysCount = radius / 2;
                        var peaksCount = radius - valleysCount;
                        return hexCenter + new int3(1 - peaksCount * 2, valleysCount, valleysCount);
                    }
                case HexEdge.BottomRight:
                    {
                        var peaksCount = radius / 2;
                        var valleysCount = radius - peaksCount;
                        return hexCenter + new int3(-peaksCount, -peaksCount, valleysCount * 2 - 1);
                    }
                case HexEdge.Bottom:
                    {
                        var valleysCount = radius / 2;
                        var peaksCount = radius - valleysCount;
                        return hexCenter + new int3(valleysCount, 1 - peaksCount * 2 , valleysCount);
                    }
                case HexEdge.BottomLeft:
                    {
                        var peaksCount = radius / 2;
                        var valleysCount = radius - peaksCount;
                        return hexCenter + new int3(valleysCount * 2 - 1 , - peaksCount, -peaksCount);
                    }
                case HexEdge.TopLeft:
                    {
                        var valleysCount = radius / 2;
                        var peaksCount = radius - valleysCount;
                        return hexCenter + new int3(valleysCount, valleysCount, 1 - peaksCount * 2);
                    }
                default:
                    {
                        var peaksCount = radius / 2;
                        var valleysCount = radius - peaksCount;
                        return hexCenter + new int3(-peaksCount,  valleysCount * 2 - 1, -peaksCount);
                    }
            }
        }

        // NOTE: getting limits is not enough, you also need a special logic object (UniversalEdgeLimitLogic)
        [BurstCompile]
        public static (int3 min, int3 max) GetEdgeTriangleLimits(this HexEdge edge, IntTriangularPos hexCenter, int radius)
        {
            switch (edge)
            {
                case HexEdge.TopRight: 
                    {
                        return (
                            new (hexCenter.X - radius, hexCenter.Y, hexCenter.Z), 
                            new (hexCenter.X - radius +1, hexCenter.Y + radius - 1, hexCenter.Z + radius -1));
                    }
                case HexEdge.BottomRight:
                    {
                        return (
                            new(hexCenter.X - radius + 1, hexCenter.Y - radius + 1, hexCenter.Z + radius - 1),
                            new(hexCenter.X, hexCenter.Y, hexCenter.Z + radius));
                    }
                case HexEdge.Bottom:
                    {
                        return (
                            new(hexCenter.X, hexCenter.Y - radius, hexCenter.Z),
                            new(hexCenter.X + radius - 1, hexCenter.Y - radius + 1, hexCenter.Z + radius - 1));
                    }
                case HexEdge.BottomLeft:
                    {
                        return (
                            new(hexCenter.X + radius - 1, hexCenter.Y - radius + 1, hexCenter.Z - radius + 1),
                            new(hexCenter.X + radius, hexCenter.Y, hexCenter.Z));
                    }
                case HexEdge.TopLeft:
                    {
                        return (
                            new(hexCenter.X, hexCenter.Y, hexCenter.Z - radius),
                            new(hexCenter.X + radius - 1, hexCenter.Y + radius - 1, hexCenter.Z - radius + 1));
                    }
                default: 
                    {
                        return (
                           new(hexCenter.X - radius + 1,hexCenter.Y + radius - 1, hexCenter.Z - radius + 1),
                           new(hexCenter.X,hexCenter.Y + radius, hexCenter.Z));
                    }
            }
        }
    }
}
