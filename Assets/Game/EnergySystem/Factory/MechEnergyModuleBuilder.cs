using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechEnergyModuleBuilder
    {
        private readonly EnergyCellsFactory _energyCellsFactory;
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;

        private readonly Stash<EnergyChargeSpeedComponent> _chargeSpeedComponents;
        private readonly Stash<MechReactorComponent> _mechReactorBindComponents;
        private readonly Stash<ReactorComponent> _reactorComponents;
        private readonly Stash<AdrenalineComponent> _adrenalineComponents;
        private readonly Stash<HealthComponent> _health;

        [Inject]
        public MechEnergyModuleBuilder(EnergyCellsFactory energyCellsFactory, World world, ParentingRelationsApplier parentingRelationsApplier)
        {
            _energyCellsFactory = energyCellsFactory;
            _world = world;
            _parentingRelationsApplier = parentingRelationsApplier;

            _chargeSpeedComponents = _world.GetStash<EnergyChargeSpeedComponent>();
            _reactorComponents = _world.GetStash<ReactorComponent>();
            _adrenalineComponents = _world.GetStash<AdrenalineComponent>();

            _mechReactorBindComponents = _world.GetStash<MechReactorComponent>();

            _health = _world.GetStash<HealthComponent>();
        }

        public void Build(Entity mechEntity, IPartitionsList partitionsList, MechConfig mechConfig)
        {
            // reactor is discrete entity
            var reactorEntity = BuildReactor(mechConfig.ReactorConfig);
            _mechReactorBindComponents.Set(mechEntity, new(reactorEntity));
            _parentingRelationsApplier.CreateSimpleParentingBond(mechEntity, reactorEntity);


            // create energy grid for each partitions
            var cellsConfig = mechConfig.EnergyCellsConfig;
            foreach (var partitionKvp in partitionsList)
            {
                if (!cellsConfig.TryGetCellsCount(partitionKvp.Key, out var cellsCount))
                    continue;

                var partitionEntity = partitionKvp.Value;
                _energyCellsFactory.BuildPartitionEnergySystem(mechEntity, partitionEntity, reactorEntity, cellsCount, cellsConfig.CellConfig);
            }

            // add adrenaline feature
            _adrenalineComponents.Set(mechEntity, new(mechConfig.ReactorConfig));

            // potential future problem: adrenaline & energy system features are too tight coupling
        }

        private Entity BuildReactor(MechReactorConfig reactorConfig)
        {
            var reactor = _world.CreateEntity();
            _chargeSpeedComponents.Set(reactor);
            _reactorComponents.Set(reactor, new(reactorConfig));
            _health.Set(reactor, new(reactorConfig.HealthPoints));
            return reactor;
        }
    
    }
}
