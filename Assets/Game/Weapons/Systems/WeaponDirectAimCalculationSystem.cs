using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponDirectAimCalculationSystem : WeaponAimCalculationSystemBase<WeaponDirectAimTag> 
    {
        public WeaponDirectAimCalculationSystem(TransformAspectHandler transformAspectHandler, SceneFlagsManager sceneFlagsManager) : base(transformAspectHandler, sceneFlagsManager)
        {
        }

        protected override void SetAim(Entity weaponEntity, bool haveTower, float3 localTargetPos)
        {
            var barrelEntity = _weaponBarrels.Get(weaponEntity).BarrelEntity;

            float3 normalizedTargetDir;
            if (haveTower)
            {
                // only up\down
                var groundVectorLength = math.length(localTargetPos.xz);
                normalizedTargetDir = math.normalize(new float3(0f, localTargetPos.y, groundVectorLength));
            }
            else
            {
                //full aim
                normalizedTargetDir = math.normalize(localTargetPos);
            }

            SetBarrelLocalAim(barrelEntity, normalizedTargetDir);
        }
    }
}