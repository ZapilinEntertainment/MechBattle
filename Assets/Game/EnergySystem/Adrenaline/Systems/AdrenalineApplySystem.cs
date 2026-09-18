using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AdrenalineApplySystem : ISystem 
    {
        public World World { get; set;}
        private Filter _mechBoostFilter;
        private Stash<AdrenalineComponent> _adrenaline;
        private Stash<EnergySourceComponent> _energySources;
        private Stash<ReactorComponent> _reactorComponents;
        private Stash<EnergyChargeSpeedComponent> _chargeSpeed;
        private Stash<RepairSpeedComponent> _repairSpeed;
        

        public void OnAwake() 
        {
            _mechBoostFilter = World.Filter
                .With<AdrenalineComponent>()
                .With<EnergySourceComponent>()
                .Build();

            _adrenaline = World.GetStash<AdrenalineComponent>();
            _energySources = World.GetStash<EnergySourceComponent>();
            _reactorComponents = World.GetStash<ReactorComponent>();
            _chargeSpeed = World.GetStash<EnergyChargeSpeedComponent>();
            _repairSpeed = World.GetStash<RepairSpeedComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var mechEntity in _mechBoostFilter)
            {
                var adrenalineComponent = _adrenaline.Get(mechEntity);

                var reactorEntity = _energySources.Get(mechEntity).SourceEntity;
                if (World.IsDisposed(reactorEntity))
                    continue;
                var reactorComponent = _reactorComponents.Get(reactorEntity);
                _chargeSpeed.Set(reactorEntity, new() { Value = reactorComponent.BaseEnergyProduceSpeed * adrenalineComponent.EnergyProductionCf });

                // todo: make a discrete component for min\max repair speed?
                // note: we set speed on mech entity, not reactor one
                _repairSpeed.Set(mechEntity, new() { Value = reactorComponent.BaseRepairSpeed * adrenalineComponent.RepairSpeedCf });
            }
        }

        public void Dispose() { }
    }
}