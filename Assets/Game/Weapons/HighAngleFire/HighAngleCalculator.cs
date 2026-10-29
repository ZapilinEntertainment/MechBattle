using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Weapons
{
    public class HighAngleCalculator
    {
        [BurstCompile]
        // logic by Deepseek
        public static bool TrySolveHighArc(
            float3 startPos, 
            float3 targetPos, 
            float projectileSpeed, 
            float g,
            out float3 targetVector, 
            out float flightTime)
        {
            targetVector = float3.zero; 
            flightTime = 0f;

            float3 delta = targetPos - startPos;
            float3 flat = new (delta.x, 0f, delta.z);
            float d = math.length(flat);
            float h = delta.y;

            if (d < 1e-3f) 
                return false; 

            float v2 = projectileSpeed * projectileSpeed;
            float D = v2 * v2 - g * (g * d * d + 2f * h * v2);
            if (D < 0f) 
                return false; 

            float tanTheta = (v2 + math.sqrt(D)) / (g * d);
            float theta = math.atan(tanTheta);
            float cos = math.cos(theta);

            if (cos < 1e-5f) return false;

            flightTime = d / (projectileSpeed * cos);
            float3 dirXZ = flat / d;
            targetVector = math.normalize(cos * dirXZ + math.sin(theta) * math.up());
            return true;
        }
    }
}
