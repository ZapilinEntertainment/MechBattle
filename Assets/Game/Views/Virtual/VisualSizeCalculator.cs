using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public static class VisualSizeCalculator
    {
        [BurstCompile]
        public static float Calculate(float3 scale)
        {
            // bounding sphere
            return 0.5f * math.sqrt(scale.x * scale.x + scale.y * scale.y + scale.z * scale.z);
        }

    }
}
