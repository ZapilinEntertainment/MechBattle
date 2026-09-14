using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WeaponChargeComponent : IComponent 
    {
        public float ChargePercent;
        public readonly float ChargeSpeed;
        public readonly float DischargeSpeed;
        public readonly float ManualReleaseMinPercent;

        public float OverchargePercent => 
            ManualReleaseMinPercent == 0f 
            ? ChargePercent 
            : math.clamp((ChargePercent - ManualReleaseMinPercent) / (1f - ManualReleaseMinPercent), 0f, 1f);

        public WeaponChargeComponent(WeaponChargeSettings settings, float startPercent = 0f)
        {
            ChargePercent = startPercent;
            ChargeSpeed = settings.ChargeTime == 0f ? float.MaxValue : 1f / settings.ChargeTime;
            DischargeSpeed = settings.DischargeTime == 0f ? float.MaxValue : 1f / settings.DischargeTime;
            ManualReleaseMinPercent = settings.ManualReleaseMinPercent;
        }
    }
}