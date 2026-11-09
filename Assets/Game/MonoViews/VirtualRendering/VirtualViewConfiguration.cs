using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(VirtualViewConfiguration), menuName = "Scriptable Objects/" + nameof(VirtualViewConfiguration))]
    public class VirtualViewConfiguration : ScriptableObject, IVirtualViewConfig
    {
        [field:SerializeField] public string MeshKey { get; private set; }
        [field: SerializeField] public string MaterialKey { get; private set; }
        [field: SerializeField] public float3 Scale { get; private set; }
        [field: SerializeField] public float VisibilityLimit { get; private set; }
    }
}
