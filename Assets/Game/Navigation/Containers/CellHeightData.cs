using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct CellHeightData
    {
        public readonly short AverageHeight;
        public int PinnacleHeight => AverageHeight + PinnacleHeightDelta;
        public int LeftBasisHeight => AverageHeight + LeftBasisHeightDelta;
        public int RightBasisHeight => AverageHeight + RightBasisHeightDelta;


        private readonly sbyte PinnacleHeightDelta;
        private readonly sbyte LeftBasisHeightDelta;
        private readonly sbyte RightBasisHeightDelta;       

        public CellHeightData(RefinedTriangleRaycastData raycastData)
        {
            var averageHeight = raycastData.AverageGroundHeight;
            AverageHeight = (short)averageHeight;
            PinnacleHeightDelta = GetDelta(raycastData.PinnacleHeight);
            LeftBasisHeightDelta = GetDelta(raycastData.LeftBasisHeight);
            RightBasisHeightDelta = GetDelta(raycastData.RightBasisHeight);

            sbyte GetDelta(float height) => (sbyte)(math.clamp(height - averageHeight, sbyte.MinValue +1, sbyte.MaxValue -1));
        }

        
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

    }
}
