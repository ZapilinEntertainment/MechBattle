using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle.Weapons
{
    public abstract class WeaponPartConfigBase : IWeaponFeatureConfig
    {
        [field: SerializeField] public bool AutoStow { get; private set; }
        [field: SerializeField] public float RotationSpeedDegrees { get; private set; }
        [field: SerializeField] public float3 LocalPosition { get; private set; }
        [field: SerializeField] public ViewPartKey ViewPartKey { get; private set; }
        [field: SerializeField] public ForwardRotationLimits FwdRotationLimits { get; private set; }
    }
}
