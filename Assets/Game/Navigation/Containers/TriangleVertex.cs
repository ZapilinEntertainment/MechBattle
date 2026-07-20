using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public enum TriangleVertex : byte
    {
        Pinnacle,
        LeftBasis,
        RightBasis    
    }

    public static class TriangleVertexExtension
    {
        [BurstCompile]
        public static int3 ToTriposOffsetVector(this TriangleVertex vertex, bool triposIsPeak ) 
        { 
            switch ( vertex )
            {
                case TriangleVertex.RightBasis: return triposIsPeak ? new(0,0,1) : new(-1,0,0);
                case TriangleVertex.LeftBasis: return triposIsPeak ? new(1,0,0) : new(0,0,-1);
                default: return triposIsPeak ? new(0,1,0) : new(0,-1,0);
            }
        }
    }
}
