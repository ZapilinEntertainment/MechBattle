using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class WeaponAimCalculationSystemBase<T> : PausableSystem
        where T : unmanaged, IWeaponAimTagComponent
    {
        protected Stash<WeaponBarrelComponent> _weaponBarrels;
        protected Stash<LocalTargetRotationComponent> _aims;

        private Filter _aimingTowerWeaponsFilter;
        private Filter _aimingBarrelWeaponsFilter;

        private Stash<WeaponTargetPositionComponent> _targetPositions;       
        private Stash<WeaponTowerComponent> _weaponTowers;
        
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public WeaponAimCalculationSystemBase(TransformAspectHandler transformAspectHandler, SceneFlagsManager sceneFlagsManager) : base(sceneFlagsManager)
        {
            _transformAspectHandler = transformAspectHandler;
        }

        public override void OnAwake()
        {
            _aimingTowerWeaponsFilter = World.Filter
                .With<WeaponTowerComponent>()
                .With<WeaponTargetPositionComponent>()
                .With<T>()
                .Build();

            _aimingBarrelWeaponsFilter = World.Filter
                .With<WeaponBarrelComponent>()
                .With<WeaponTargetPositionComponent>()
                .With<T>()
                .Build();

            _targetPositions = World.GetStash<WeaponTargetPositionComponent>();
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
                var targetPos = _targetPositions.Get(weaponEntity).Value;

                var weaponPoint = _transformAspectHandler.GetPoint(weaponEntity);

                var targetLocalPos = MathExtensions.InverseTransformPoint(targetPos, weaponPoint.pos, weaponPoint.rot);
                var normalizedTargetDir = math.normalize(new float3(targetLocalPos.x, 0f, targetLocalPos.z));

                var targetRotation = math.normalize(quaternion.LookRotation(normalizedTargetDir, math.up()));
                _aims.Set(towerEntity, new() { Value = targetRotation });
            }

            foreach (var weaponEntity in _aimingBarrelWeaponsFilter)
            {
                var targetPos = _targetPositions.Get(weaponEntity).Value;
                var towerComponent = _weaponTowers.Get(weaponEntity, out var haveTower);
                var parentPoint = _transformAspectHandler.GetPoint(haveTower ? towerComponent.TowerEntity : weaponEntity);
                var localTargetPos = MathExtensions.InverseTransformPoint(targetPos, parentPoint.pos, parentPoint.rot);

                SetAim(weaponEntity, haveTower, localTargetPos);
            }
        }

        protected abstract void SetAim(Entity weaponEntity, bool haveTower, float3 localTargetPos);

        protected void SetBarrelLocalAim(Entity barrelEntity, float3 normalizedTargetDir)
        {
            var localRotation = math.normalize(quaternion.LookRotation(normalizedTargetDir, math.up()));
            _aims.Set(barrelEntity, new() { Value = localRotation });
        }
    }
}