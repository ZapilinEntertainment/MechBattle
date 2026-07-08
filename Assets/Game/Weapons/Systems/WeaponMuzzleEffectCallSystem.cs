using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponMuzzleEffectCallSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<WeaponMuzzleEffectComponent> _muzzleEffects;
        private Stash<WeaponShotPoint> _shotPoints;
        private readonly VfxRequestsFactory _requestsFactory;

        [Inject]
        public WeaponMuzzleEffectCallSystem(VfxRequestsFactory vfxRequests)
        {
            _requestsFactory = vfxRequests;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<WeaponFireTag>().With<WeaponMuzzleEffectComponent>().Build();
            _muzzleEffects = World.GetStash<WeaponMuzzleEffectComponent>();
            _shotPoints = World.GetStash<WeaponShotPoint>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var weaponEntity in _filter)
            {
                var vfxKey = _muzzleEffects.Get(weaponEntity).VfxKey;
                var point = _shotPoints.Get(weaponEntity).WorldPoint;
                _requestsFactory.Build(vfxKey, point);
            }
        }

        public void Dispose() { }
    }
}