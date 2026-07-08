using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponAutoShotSystem : PausableSystem
    {
        // note: add tower-only and barrel-only filters when needed
        private Filter _towerBarrelWeapons;
        private Stash<WeaponTowerAimPrecisionComponent> _towerPrecision;
        private Stash<WeaponBarrelAimPrecisionComponent> _barrelPrecision;
        private Stash<WeaponFireTag> _fireTags;


        [Inject]
        public WeaponAutoShotSystem( SceneFlagsManager flags) : base(flags) { }

        public override void OnAwake()
        {
            _towerBarrelWeapons = World.Filter
                .With<WeaponAutoShotTag>()
                .With<WeaponTowerComponent>()
                .With<WeaponBarrelComponent>()
                .With<ReadyToShotTag>()
                .Without<EntityDisposeTag>()
                .Without<WeaponFireTag>()
                .Build();

            _towerPrecision = World.GetStash<WeaponTowerAimPrecisionComponent>();
            _barrelPrecision = World.GetStash<WeaponBarrelAimPrecisionComponent>();
            _fireTags = World.GetStash<WeaponFireTag>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var weaponEntity in _towerBarrelWeapons)
            {
                var towerAimReady = _towerPrecision.Get(weaponEntity).IsInsideLimit;
                var barrelAimReady = _barrelPrecision.Get(weaponEntity).IsInsideLimit;

                if (towerAimReady & barrelAimReady)
                {
                    // note: there can be added some delay
                    // also note: fire tag can stay for a some time (ex.: laser weapons)
                    _fireTags.Set(weaponEntity);
                }
            }
        }

      
    }
}