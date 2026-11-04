using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Projectiles
{
    public readonly struct TrajectorySegment
    {
        public readonly float3 Start;   
        public readonly float3 Dir;    
        public readonly float Length;

        public float3 End => Start + Dir * Length;

        public TrajectorySegment(float3 start, float3 end)
        {
            Start = start;
            var d = end - start;
            Length = math.length(d);
            Dir = Length > 1e-6f ? d / Length : math.forward();
        }

        public RaycastCommand ToRaycastCommand(QueryParameters queryParameters) => new(Start, Dir, queryParameters, Length);
    }
}
