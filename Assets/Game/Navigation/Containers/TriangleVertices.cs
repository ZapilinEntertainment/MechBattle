using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    // used for debug triangle display
    public readonly struct TriangleVertices
    {
        public readonly float3 A;
        public readonly float3 B;
        public readonly float3 C;

        public TriangleVertices(float3 a, float3 b, float3 c)
        {
            A = a;
            B = b;
            C = c;
        }

        public TriangleVertices(float2 a, float2 b, float2 c)
        {
            A = new(a.x, 0f, a.y);
            B = new(b.x, 0f, b.y);
            C = new(c.x, 0f, c.y);
        }
    }
}
