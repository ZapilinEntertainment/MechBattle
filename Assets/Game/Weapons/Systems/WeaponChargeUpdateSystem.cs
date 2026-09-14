using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponChargeUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _chargingFilter;
        private Filter _dischargingFilter;
        private Stash<WeaponChargeComponent> _chargeComponents;
        private readonly WeaponHandler _weaponHandler;

        [Inject]
        public WeaponChargeUpdateSystem(WeaponHandler weaponHandler)
        {
            _weaponHandler = weaponHandler;
        }

        public void OnAwake() 
        {
            _chargingFilter = World.Filter
                .With<WeaponChargeComponent>()
                .With<ChargingWeaponTag>()
                .Build();

            _dischargingFilter = World.Filter
                .With<WeaponChargeComponent>()
                .With<ContinuosFiringTag>()
                .With<WeaponFireTag>()
                .Build();

            _chargeComponents = World.GetStash<WeaponChargeComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var weaponEntity in _chargingFilter)
            {
                ref var chargeComponent = ref _chargeComponents.Get(weaponEntity);
                var percent = math.clamp(chargeComponent.ChargePercent + chargeComponent.ChargeSpeed * deltaTime, 0f, 1f);
                chargeComponent.ChargePercent = percent;
                if (percent == 1f)
                    _weaponHandler.ReleaseChargingWeapon(weaponEntity);
            }

            foreach (var weaponEntity in _dischargingFilter)
            {
                ref var chargeComponent = ref _chargeComponents.Get(weaponEntity);
                var percent = math.clamp(chargeComponent.ChargePercent - chargeComponent.DischargeSpeed * deltaTime, 0f, 1f);
                chargeComponent.ChargePercent = percent;
                if (percent == 0f)
                    _weaponHandler.StopWeapongFiring(weaponEntity);

            }
        }

        public void Dispose() { }
    }
}