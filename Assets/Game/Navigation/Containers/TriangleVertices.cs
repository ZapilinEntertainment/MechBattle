using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct TriangleVertices
    {
        public readonly float3 PinnaclePos;
        public readonly float3 LeftBasisPos;
        public readonly float3 RightBasisPos;

        public TriangleVertices(float3 pinnacle, float3 leftBasis, float3 rightBasis)
        {
            PinnaclePos = pinnacle;
            LeftBasisPos = leftBasis;
            RightBasisPos = rightBasis;
        }

        public TriangleVertices(float2 pinnacle, float2 leftBasis, float2 rightBasis)
        {
            PinnaclePos = new(pinnacle.x, 0f, pinnacle.y);
            LeftBasisPos = new(leftBasis.x, 0f, leftBasis.y);
            RightBasisPos = new(rightBasis.x, 0f, rightBasis.y);
        }

        public TriangleVertices ApplyHeights(float4 heights)
        {
            var pinnacle = new float3(PinnaclePos.x, heights[(int)TriangleHeightMeasurePoint.Pinnacle], PinnaclePos.z);
            var leftBasis = new float3(LeftBasisPos.x, heights[(int)TriangleHeightMeasurePoint.LeftBasis], LeftBasisPos.z);
            var rightBasis = new float3(RightBasisPos.x, heights[(int)TriangleHeightMeasurePoint.RightBasis], RightBasisPos.z);
            return new(pinnacle, leftBasis, rightBasis);
        }

        public TriangleVertices AddHeight(float value)
        {
            var delta = new float3(0,value, 0);
            return new(PinnaclePos + delta, LeftBasisPos + delta, RightBasisPos + delta);
        }

        public void AddPointsToList(in IList<(float3 start, float3 end)> list)
        {
            list.Add((PinnaclePos, LeftBasisPos));
            list.Add((LeftBasisPos, RightBasisPos));
            list.Add((PinnaclePos, RightBasisPos));
        }
    }
}
