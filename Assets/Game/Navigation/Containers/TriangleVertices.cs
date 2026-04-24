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

        public static TriangleVertices ConstructWithDefaultHeight(float2 pinnacle, float2 leftBasis, float2 rightBasis) =>        
            new (
                new(pinnacle.x, NavigationConstants.DEFAULT_HEIGHT, pinnacle.y),
                new(leftBasis.x, NavigationConstants.DEFAULT_HEIGHT, leftBasis.y),
                new(rightBasis.x, NavigationConstants.DEFAULT_HEIGHT, rightBasis.y));
        

        public TriangleVertices ApplyHeights(float4 heights)
        {
            var pinnacle = new float3(PinnaclePos.x, heights[(int)TriangleHeightMeasurePoint.Pinnacle], PinnaclePos.z);
            var leftBasis = new float3(LeftBasisPos.x, heights[(int)TriangleHeightMeasurePoint.LeftBasis], LeftBasisPos.z);
            var rightBasis = new float3(RightBasisPos.x, heights[(int)TriangleHeightMeasurePoint.RightBasis], RightBasisPos.z);
            return new(pinnacle, leftBasis, rightBasis);
        }

        public TriangleVertices ApplyHeights(CellHeightData heights)
        {
            var pinnacle = new float3(PinnaclePos.x, heights.PinnacleHeight, PinnaclePos.z);
            var leftBasis = new float3(LeftBasisPos.x, heights.LeftBasisHeight, LeftBasisPos.z);
            var rightBasis = new float3(RightBasisPos.x, heights.RightBasisHeight, RightBasisPos.z);
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
