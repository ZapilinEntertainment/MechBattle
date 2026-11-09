using Unity.Mathematics;

namespace ZE.MechBattle
{
    public interface IVirtualViewConfig
    {
        string MeshKey { get; }
        string MaterialKey { get; }
        float3 Scale { get; }
        float VisibilityLimit { get; }
    }
}
