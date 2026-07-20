using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public readonly struct CellHeightData : ICellHeightData
    {
        public readonly short AverageHeight;
        public int PinnacleHeight => AverageHeight + PinnacleHeightDelta;
        public int LeftBasisHeight => AverageHeight + LeftBasisHeightDelta;
        public int RightBasisHeight => AverageHeight + RightBasisHeightDelta;

        float ICellHeightData.PinnacleHeight => PinnacleHeight;
        float ICellHeightData.LeftBasisHeight => LeftBasisHeight;
        float ICellHeightData.RightBasisHeight => RightBasisHeight;
        float ICellHeightData.AverageHeight => AverageHeight;

        private readonly sbyte PinnacleHeightDelta;
        private readonly sbyte LeftBasisHeightDelta;
        private readonly sbyte RightBasisHeightDelta;       

        public CellHeightData(RefinedTriangleRaycastData raycastData)
        {
            var averageHeight = raycastData.AverageHeight;
            AverageHeight = (short)averageHeight;
            PinnacleHeightDelta = GetDelta(raycastData.PinnacleHeight);
            LeftBasisHeightDelta = GetDelta(raycastData.LeftBasisHeight);
            RightBasisHeightDelta = GetDelta(raycastData.RightBasisHeight);

            sbyte GetDelta(float height) => (sbyte)(math.clamp(height - averageHeight, sbyte.MinValue +1, sbyte.MaxValue -1));
        }

        public CellHeightData(float singleHeight)
        {
            AverageHeight = (short)singleHeight;
            PinnacleHeightDelta = 0;
            LeftBasisHeightDelta = 0;
            RightBasisHeightDelta = 0;
        }

        private CellHeightData(short averageHeight, sbyte pinnacleDelta, sbyte leftBasisDelta, sbyte rightBasisDelta)
        {
            AverageHeight = averageHeight;
            PinnacleHeightDelta = pinnacleDelta;
            LeftBasisHeightDelta = leftBasisDelta;
            RightBasisHeightDelta = rightBasisDelta;
        }

        public CellHeightData AddHeight(float delta) => new((short)(AverageHeight + delta), PinnacleHeightDelta, LeftBasisHeightDelta, RightBasisHeightDelta);

        
        public float4 ToCombinedValue()
        {
            float4 val = new();
            // order is important:
            val[(int)TriangleHeightMeasurePoint.Average] = AverageHeight;
            val[(int)TriangleHeightMeasurePoint.Pinnacle] = PinnacleHeight;
            val[(int)TriangleHeightMeasurePoint.LeftBasis] = LeftBasisHeight;
            val[(int)TriangleHeightMeasurePoint.RightBasis] = RightBasisHeight;
            return val;
        }

        public int this[TriangleHeightMeasurePoint measurePoint]
        {
            get
            {
                switch(measurePoint)
                {
                    case TriangleHeightMeasurePoint.Pinnacle: return PinnacleHeight;
                    case TriangleHeightMeasurePoint.LeftBasis: return LeftBasisHeight;
                    case TriangleHeightMeasurePoint.RightBasis: return RightBasisHeight;
                    default: return AverageHeight;
                }
            }
        }

        public float GetHeightAtPoint(IntTriangularPos tripos, float3 localTripos)
        {
            var vertexWeights = CellLogic.GetVertexWeights(tripos, localTripos);
            return vertexWeights.PinnacleWeight * PinnacleHeight 
                + vertexWeights.LeftBasisWeight * LeftBasisHeight 
                + vertexWeights.RightBasisWeight * RightBasisHeight;
        }

    }
}
