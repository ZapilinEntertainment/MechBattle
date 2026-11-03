using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // stowing inactive weapons back to default rotation
    public sealed class WeaponStowSystem : PausableSystem
    {
        private Filter _idleTowerWeaponsFilter;
        private Filter _idleBarrelWeaponsFilter;

        private Stash<LocalTargetRotationComponent> _aims;


        public WeaponStowSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _idleTowerWeaponsFilter = World.Filter
                .With<WeaponTowerStowTag>()
                .Without<WeaponTargetPositionComponent>()
                .Build();

            _idleBarrelWeaponsFilter = World.Filter
                .With<WeaponBarrelStowTag>()
                .Without<WeaponTargetPositionComponent>()
                .Build();

            _aims = World.GetStash<LocalTargetRotationComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var towerEntity in _idleTowerWeaponsFilter)
            {
                _aims.Set(towerEntity, new() { Value = quaternion.identity });
            }

            foreach (var barrelEntity in _idleBarrelWeaponsFilter)
            {
                _aims.Set(barrelEntity, new() { Value = quaternion.identity });
            }
        }
    }
}