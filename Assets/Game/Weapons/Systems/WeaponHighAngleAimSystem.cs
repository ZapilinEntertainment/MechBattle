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
        private Stash<ProjectileSpeedComponent> _projectileMoveSpeed;
        private Stash<HighAimResultsComponent> _highAimResults;
        private Stash<WeaponShotPoint> _shotPoint;
        private Stash<AttackTargetComponent> _attackTargets;

        public WeaponHighAngleAimSystem(TransformAspectHandler transformAspectHandler, SceneFlagsManager sceneFlagsManager) : base(transformAspectHandler, sceneFlagsManager)
        {
        }

        public override void OnAwake()
        {
            base.OnAwake();

            _projectileMoveSpeed = World.GetStash<ProjectileSpeedComponent>();
            _highAimResults = World.GetStash<HighAimResultsComponent>();
            _shotPoint = World.GetStash<WeaponShotPoint>();
            _attackTargets = World.GetStash<AttackTargetComponent>();
        }

        protected override FilterBuilder GetBarrelFilterBuilder()
        {
            return base.GetBarrelFilterBuilder().With<WeaponShotPoint>();
        }

        protected override void SetBarrelAim(Entity weaponEntity)
        {
            var targetPos = _targetPositions.Get(weaponEntity).Value;           
            var projectileSpeed = _projectileMoveSpeed.Get(weaponEntity).Value;

            var shotPoint = _shotPoint.Get(weaponEntity).WorldPoint.pos;

            var towerComponent = _weaponTowers.Get(weaponEntity, out var haveTower);
            var parentPoint = _transformAspectHandler.GetPoint(haveTower ? towerComponent.TowerEntity : weaponEntity);

            if (HighAngleTrajectorySolver.TryExecute(shotPoint, targetPos, projectileSpeed, -Physics.gravity.y, out var normalizedTargetVector, out var flightTime)) 
            {
                var barrelEntity = _weaponBarrels.Get(weaponEntity).BarrelEntity;
              

                float3 normalizedTargetDir;
                var localTargetVector = math.mul(math.inverse(parentPoint.rot), normalizedTargetVector);

                haveTower = false;
                if (haveTower)
                {
                    // with tower -  only up / down:
                    var groundVectorLength = math.length(localTargetVector.xz);
                    normalizedTargetDir = math.normalize(new float3(0f, localTargetVector.y, groundVectorLength));
                }
                else
                {
                    // without tower - full 3d aim
                    normalizedTargetDir = math.normalize(localTargetVector);
                }

                SetBarrelLocalAim(barrelEntity, normalizedTargetDir);
                _highAimResults.Set(weaponEntity, new() { FlightTime = flightTime, NormalizedDirectionVector = normalizedTargetVector });
            }
            else
            {
                _highAimResults.Remove(weaponEntity);
                _attackTargets.Remove(weaponEntity);
            }
        }
    }
}