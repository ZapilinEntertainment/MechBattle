using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponStopFireSystem : PausableSystem
    {
        private Filter _filter;
        private Stash<WeaponFireTag> _stash;
        private readonly IWeaponShotCompletenessHandler _completenessHandler;

        public WeaponStopFireSystem(IWeaponShotCompletenessHandler completenessHandler, SceneFlagsManager flags) : base(flags)
        {
            _completenessHandler = completenessHandler;
        }

        public override void OnAwake()
        {
            _filter = World.Filter
                .With<WeaponFireTag>()
                .Without<ContinuosFiringTag>()
                .Build();
            _stash = World.GetStash<WeaponFireTag>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var weaponEntity in _filter)
            {
                _completenessHandler.OnWeaponShot(weaponEntity);
                _stash.Remove(weaponEntity);
            }
        }
    }
}