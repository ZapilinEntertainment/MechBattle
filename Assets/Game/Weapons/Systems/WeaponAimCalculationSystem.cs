using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.Utils;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponAimCalculationSystem : PausableSystem 
    {
        private Filter _aimingTowerWeaponsFilter;
        private Filter _aimingbarrelWeaponsFilter;
        private Filter _idleTowerWeaponsFilter;
        private Filter _idleBarrelWeaponsFilter;

        private Stash<AttackTargetComponent> _attackTargetComponent;
        private Stash<PositionComponent> _positions;
        private Stash<RotationComponent> _rotations;
        private Stash<LocalTargetRotationComponent> _aims;
        private Stash<WeaponTowerComponent> _weaponTowers;
        private Stash<WeaponBarrelComponent> _weaponBarrels;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public WeaponAimCalculationSystem(TransformAspectHandler transformAspectHandler, SceneFlagsManager sceneFlagsManager) : base(sceneFlagsManager) 
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public override void OnAwake() 
        {
            _aimingTowerWeaponsFilter = World.Filter
                .With<WeaponTowerComponent>()
                .With<AttackTargetComponent>()
                .Build();
            _aimingbarrelWeaponsFilter = World.Filter
                .With<WeaponBarrelComponent>()
                .With<AttackTargetComponent>()
                .Build();

            _idleTowerWeaponsFilter = World.Filter
                .With<WeaponTowerStowTag>()
                .Without<AttackTargetComponent>()
                .Build();
            _idleBarrelWeaponsFilter = World.Filter
                .With<WeaponBarrelStowTag>()
                .Without<AttackTargetComponent>()
                .Build();

            _attackTargetComponent = World.GetStash<AttackTargetComponent>();   
            _positions = World.GetStash<PositionComponent>();
            _rotations = World.GetStash<RotationComponent>();
            _aims = World.GetStash<LocalTargetRotationComponent>();

            _weaponTowers = World.GetStash<WeaponTowerComponent>();
            _weaponBarrels = World.GetStash<WeaponBarrelComponent>();
        }

        public override void OnUpdate(float deltaTime) 
        {
            if (IsPaused) return;

            foreach (var weaponEntity in _aimingTowerWeaponsFilter)
            {
                var towerEntity = _weaponTowers.Get(weaponEntity).TowerEntity;
                var targetEntity = _attackTargetComponent.Get(weaponEntity).Entity;
                var targetPos = _positions.Get(targetEntity).Value;

                var weaponPoint = _transformAspectHandler.GetPoint(weaponEntity);
                var targetLocalPos = MathExtensions.InverseTransformPoint(targetPos, weaponPoint.pos, weaponPoint.rot);
                var normalizedTargetDir = math.normalize(new float3(targetLocalPos.x, 0f, targetLocalPos.z));

                var targetRotation = quaternion.LookRotation(normalizedTargetDir, math.up());
                _aims.Set(towerEntity, new() { Value = targetRotation });
            }

            foreach (var weaponEntity in _aimingbarrelWeaponsFilter)
            {
                var barrelEntity = _weaponBarrels.Get(weaponEntity).BarrelEntity;
                var targetEntity = _attackTargetComponent.Get(weaponEntity).Entity;
                var targetPos = _positions.Get(targetEntity).Value;

                var towerComponent = _weaponTowers.Get(weaponEntity, out var hasTower);
                var parentPoint = _transformAspectHandler.GetPoint(hasTower ? towerComponent.TowerEntity : weaponEntity);                
                var localTargetPos = MathExtensions.InverseTransformPoint(targetPos, parentPoint.pos, parentPoint.rot);

                float3 normalizedTargetDir;
                if (hasTower)
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

                var targetRotation = quaternion.LookRotation(normalizedTargetDir, math.up());
                _aims.Set(barrelEntity, new() { Value = targetRotation});               
            }



            foreach (var towerEntity in _idleTowerWeaponsFilter)
            {
                _aims.Set(towerEntity, new() { Value = quaternion.identity});
            }

            foreach (var barrelEntity in _idleBarrelWeaponsFilter)
            {
                _aims.Set(barrelEntity, new() { Value = quaternion.identity });
            }
        }
    }
}