using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WeaponChargeEnergyConsumeSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _chargeSupplyFilter;
        private Filter _dischargeSupplyFilter;
        private Stash<ChargingWeaponTag> _chargingWeaponTag;
        private Stash<WeaponFireTag> _firingTag;
        private Stash<WeaponChargeEnergyConsumption> _chargeConsumption;
        private Stash<WeaponDischargeEnergyConsumption> _dischargeConsumption;
        private Stash<EnergySourceComponent> _energySources;
        private Stash<WeaponChargeComponent> _chargeComponent;

        private readonly EnergyHandler _energyHandler;

        [Inject]
        public WeaponChargeEnergyConsumeSystem(EnergyHandler energyHandler)
        {
            _energyHandler = energyHandler;
        }

        public void OnAwake() 
        {
            _chargeSupplyFilter = World.Filter
                .With<WeaponChargeComponent>()
                .With<WeaponChargeEnergyConsumption>()
                .With<ChargingWeaponTag>()
                .Build();

            _dischargeSupplyFilter = World.Filter
                .With<WeaponChargeComponent>()
                .With<WeaponDischargeEnergyConsumption>()
                .With<ContinuosFiringTag>()
                .With<WeaponFireTag>()
                .Build();

            _chargingWeaponTag = World.GetStash<ChargingWeaponTag>();
            _firingTag = World.GetStash<WeaponFireTag>();
            _chargeConsumption = World.GetStash<WeaponChargeEnergyConsumption>();
            _dischargeConsumption = World.GetStash<WeaponDischargeEnergyConsumption>();
            _energySources = World.GetStash<EnergySourceComponent>();
            _chargeComponent = World.GetStash<WeaponChargeComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var weaponEntity in _chargeSupplyFilter)
            {
                var cost = _chargeConsumption.Get(weaponEntity).Volume * deltaTime;
                if (!_energyHandler.TrySpendEnergyForEntity(weaponEntity, cost, out var shortage))
                    AbortCharging(weaponEntity);
            }

            foreach (var weaponEntity in _dischargeSupplyFilter)
            {
                var cost = _dischargeConsumption.Get(weaponEntity).Volume * deltaTime;
                if (!_energyHandler.TrySpendEnergyForEntity(weaponEntity, cost, out var shortage))
                    AbortDischarging(weaponEntity);
            }
        }

        public void Dispose() { }

        private void AbortCharging(Entity weaponEntity)
        {
            _chargingWeaponTag.Remove(weaponEntity);
            _chargeComponent.Get(weaponEntity).ChargePercent = 0f;
        }
        private void AbortDischarging(Entity weaponEntity)
        {
            _firingTag.Remove(weaponEntity);
            _chargeComponent.Get(weaponEntity).ChargePercent = 0f;
        }
    }
}