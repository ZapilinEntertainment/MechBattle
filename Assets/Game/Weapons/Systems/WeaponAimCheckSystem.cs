using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponAimCheckSystem : PausableSystem
    {
        private Filter _towerFilter;
        private Filter _barrelFilter;
        private Stash<WeaponTowerComponent> _towerComponents;
        private Stash<WeaponTowerAimTargetComponent> _towerAimTargets;
        private Stash<WeaponTowerAimPrecisionComponent> _towerPrecision;

        private Stash<WeaponBarrelComponent> _barrelComponents;
        private Stash<WeaponBarrelAimTargetComponent> _barrelAimTargets;
        private Stash<WeaponBarrelAimPrecisionComponent> _barrelPrecision;

        public WeaponAimCheckSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _towerFilter = World.Filter
                .With<WeaponTowerComponent>()
                .With<WeaponTowerAimPrecisionComponent>()
                .Build();
            _towerComponents = World.GetStash<WeaponTowerComponent>();
            _towerAimTargets = World.GetStash<WeaponTowerAimTargetComponent>();
            _towerPrecision = World.GetStash<WeaponTowerAimPrecisionComponent>();

            _barrelFilter = World.Filter
                .With<WeaponBarrelComponent>()
                .With<WeaponBarrelAimPrecisionComponent>()
                .Build();
            _barrelComponents = World.GetStash<WeaponBarrelComponent>();
            _barrelAimTargets = World.GetStash<WeaponBarrelAimTargetComponent>();
            _barrelPrecision = World.GetStash<WeaponBarrelAimPrecisionComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var weaponEntity in _towerFilter)
            {
                var towerAngle = _towerComponents.Get(weaponEntity).RotationRadianValue;
                var towerAim = _towerAimTargets.Get(weaponEntity).RadianValue;
                ref var precisionComponent = ref _towerPrecision.Get(weaponEntity);
                precisionComponent.IsInsideLimit = math.abs(towerAngle - towerAim) < precisionComponent.PrecisionLimit;
            }

            foreach (var weaponEntity in _barrelFilter)
            {
                var barrelComponent = _barrelComponents.Get(weaponEntity);
                var barrelAngle = barrelComponent.RadianRotation;
                var barrelAim = _barrelAimTargets.Get(weaponEntity).RadianValueV2;
                ref var precisionComponent = ref _barrelPrecision.Get(weaponEntity);

                var xRotationInsideLimit = math.abs(barrelAngle.x - barrelAim.x) < precisionComponent.PrecisionLimit;
                var yRotationInsideLimit = barrelComponent.YRotationPossible ? math.abs(barrelAngle.y - barrelAim.y) < precisionComponent.PrecisionLimit : true;

                precisionComponent.IsInsideLimit = xRotationInsideLimit & yRotationInsideLimit;
            }
        }
    }
}