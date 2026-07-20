using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class HexLogic
    {
        [BurstCompile]
        public static IntTriangularPos GetClosestVirtualHexPos(float3 worldPos, float triangleHeight) 
        {
            var floatTripos = TriangularMath.WorldToTriangular(worldPos, triangleHeight);
            var intTripos = TriangularMath.WorldToTrianglePos(worldPos, triangleHeight);
            var closestVertex = CellLogic.GetClosestVertex(intTripos, floatTripos);

            int3 delta = int3.zero;
            var isPeak = intTripos.IsPeak;
            switch (closestVertex)
            {
                case TriangleVertex.RightBasis: delta = isPeak ? new int3(-1,0,0) : new int3(0,0,1); break;
                case TriangleVertex.LeftBasis: delta = isPeak ? new int3(0,0,-1) : new int3(-1,0,0); break;
                default: delta = isPeak ? new int3(0,1,0) : new int3(0,-1,0); break;
            }
            return intTripos + delta;
        }
    
    }
}
