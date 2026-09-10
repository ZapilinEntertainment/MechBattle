using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class EnergyChargingSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<EnergyChargeComponent> _chargeComponents;
        private Stash<EnergyChargeSpeedComponent> _chargeSpeedComponents;

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<EnergyChargeComponent>()
                .With<EnergyChargeSpeedComponent>()
                .Without<RepairRequiredTag>()
                .Without<EntityDisposeTag>()
                .Build();

            _chargeComponents = World.GetStash<EnergyChargeComponent>();
            _chargeSpeedComponents = World.GetStash<EnergyChargeSpeedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                ref var chargeComponent = ref _chargeComponents.Get(entity);
                if (chargeComponent.ChargePercent == 1f)
                    continue;

                var speed = _chargeSpeedComponents.Get(entity).Value;
                chargeComponent.Value = math.clamp(chargeComponent.Value + speed * deltaTime, 0f, chargeComponent.MaxValue);
            }
        }

        public void Dispose() { }
    }
}