using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using ZE.MechBattle.Weapons;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponHighAngleAimSystem : WeaponAimCalculationSystemBase<WeaponHighAngleAimTag>
    {
        private Stash<ProjectileMoveSpeed> _projectileMoveSpeed;

        public WeaponHighAngleAimSystem(TransformAspectHandler transformAspectHandler, SceneFlagsManager sceneFlagsManager) : base(transformAspectHandler, sceneFlagsManager)
        {
        }

        public override void OnAwake()
        {
            base.OnAwake();
            _projectileMoveSpeed = World.GetStash<ProjectileMoveSpeed>();
        }

        protected override void SetAim(Entity weaponEntity, bool haveTower, float3 localTargetPos)
        {
            float3 normalizedTargetDir;
            var projectileSpeed = _projectileMoveSpeed.Get(weaponEntity).Value;
            var barrelEntity = _weaponBarrels.Get(weaponEntity).BarrelEntity;

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

            if (HighAngleCalculator.TrySolveHighArc(float3.zero, normalizedTargetDir, projectileSpeed, Physics.gravity.y, out var targetVector, out var flightTime))
            {
                SetBarrelLocalAim(barrelEntity, targetVector);
            }
        }
    }
}