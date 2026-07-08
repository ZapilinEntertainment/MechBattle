using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponAimUpdateSystem : PausableSystem
    {
        private Filter _towerFilter;
        private Filter _barrelFilter;

        private Stash<WeaponTowerAimTargetComponent> _towerAimTargets;
        private Stash<WeaponTowerComponent> _towerComponents;
        private Stash<WeaponBarrelAimTargetComponent> _barrelAimTargets;
        private Stash<WeaponBarrelComponent> _barrelComponents;

        public WeaponAimUpdateSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _towerFilter = World.Filter
                .With<WeaponTowerComponent>()
                .With<WeaponTowerAimTargetComponent>()
                .Build();

            _barrelFilter = World.Filter
                .With<WeaponBarrelComponent>()
                .With<WeaponBarrelAimTargetComponent>()
                .Build();

            _towerAimTargets = World.GetStash<WeaponTowerAimTargetComponent>();
            _towerComponents = World.GetStash<WeaponTowerComponent>();
            _barrelAimTargets = World.GetStash<WeaponBarrelAimTargetComponent>();
            _barrelComponents = World.GetStash<WeaponBarrelComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var weaponEntity in _towerFilter)
            {
                ref var towerComponent = ref _towerComponents.Get(weaponEntity);
                var aimTarget = _towerAimTargets.Get(weaponEntity).RadianValue;

                towerComponent.RotationRadianValue = MathExtensions.MoveTowards(towerComponent.RotationRadianValue, aimTarget, towerComponent.RadianRotationSpeed * deltaTime);
            }

            foreach (var weaponEntity in _barrelFilter)
            {
                ref var barrelComponent = ref _barrelComponents.Get(weaponEntity);
                var aimTarget = _barrelAimTargets.Get(weaponEntity).RadianValueV2;

                var rotationX = MathExtensions.MoveTowards(barrelComponent.RadianRotation.x, aimTarget.x, barrelComponent.RadianRotationSpeed.x * deltaTime);
                var rotationY = barrelComponent.YRotationPossible ? MathExtensions.MoveTowards(barrelComponent.RadianRotation.y, aimTarget.y, barrelComponent.RadianRotationSpeed.y * deltaTime) : barrelComponent.RadianRotation.y;


                barrelComponent.RadianRotation = new float2(rotationX, rotationY);
            }
        }
    }
}