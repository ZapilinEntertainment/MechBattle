using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AimCheckSystem : PausableSystem
    {
        private Filter _filter;
        private Stash<AimPrecisionComponent> _aimPrecisionComponents;
        private Stash<LocalTargetRotationComponent> _localTargetRotations;
        private Stash<LocalRotationComponent> _localRotations;
        private Stash<WeaponTowerComponent> _towerComponents;
        private Stash<WeaponBarrelComponent> _barrelComponents;

        public AimCheckSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _filter = World.Filter
                .With<AttackTargetComponent>()
                .With<WeaponTowerComponent>()
                .With<WeaponBarrelComponent>()
                .With<AimPrecisionComponent>()
                .Build();

            _localRotations = World.GetStash<LocalRotationComponent>();
            _localTargetRotations = World.GetStash<LocalTargetRotationComponent>();
            _aimPrecisionComponents = World.GetStash<AimPrecisionComponent>();

            _towerComponents = World.GetStash<WeaponTowerComponent>();
            _barrelComponents = World.GetStash<WeaponBarrelComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var weaponEntity in _filter)
            {
                var towerComponent = _towerComponents.Get(weaponEntity, out var haveTower);
                var barrelComponent = _barrelComponents.Get(weaponEntity, out var haveBarrel);

                ref var precisionComponent = ref _aimPrecisionComponents.Get(weaponEntity);

                var towerInLimit = haveTower ? IsWeaponPartInLimit(towerComponent.TowerEntity, precisionComponent.PrecisionLimit) : true;
                var barrelInLimit = haveBarrel ? IsWeaponPartInLimit(barrelComponent.BarrelEntity, precisionComponent.PrecisionLimit) : true;

                //UnityEngine.Debug.Log($"tower: {math.angle(_localRotations.Get(towerComponent.TowerEntity).Value, _localTargetRotations.Get(towerComponent.TowerEntity).Value)}, barrelComponent: {math.angle(_localRotations.Get(barrelComponent.BarrelEntity).Value, _localTargetRotations.Get(barrelComponent.barrelEntity).Value)}");

                precisionComponent.IsInsideLimit = towerInLimit & barrelInLimit;
            }
        }

        private bool IsWeaponPartInLimit(Entity weaponPartEntity, float limit)
        {
            var localRotation = _localRotations.Get(weaponPartEntity).Value;
            var targetRotation = _localTargetRotations.Get(weaponPartEntity).Value;
            return math.abs( math.angle(localRotation, targetRotation) - limit) < math.EPSILON;
        }
    }
}