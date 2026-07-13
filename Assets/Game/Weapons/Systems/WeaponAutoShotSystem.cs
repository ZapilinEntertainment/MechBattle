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
        private Filter _filter;
        private Stash<AimPrecisionComponent> _aimPrecisions;
        private Stash<WeaponFireTag> _fireTags;


        [Inject]
        public WeaponAutoShotSystem( SceneFlagsManager flags) : base(flags) { }

        public override void OnAwake()
        {
            _filter = World.Filter
                .With<WeaponAutoShotTag>()
                .With<AimPrecisionComponent>()
                .With<ReadyToShotTag>()
                .Without<EntityDisposeTag>()
                .Without<WeaponFireTag>()
                .Build();

            _fireTags = World.GetStash<WeaponFireTag>();
            _aimPrecisions = World.GetStash<AimPrecisionComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            foreach (var weaponEntity in _filter)
            {
                if (_aimPrecisions.Get(weaponEntity).IsInsideLimit)
                {
                    // note: there can be added some delay
                    // also note: fire tag can stay for a some time (ex.: laser weapons)
                    _fireTags.Set(weaponEntity);
                }
            }
        }

      
    }
}