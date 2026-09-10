using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MechChargeCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<EnergyChargeComponent> _charges;
        private Stash<EnergyCellsGridComponent> _energyCellsGrid;
        private Stash<NextEnergyCellComponent> _nextEnergyCell;
        private readonly PartitionsListManager _partitionsManager;

        [Inject]
        public MechChargeCalculationSystem(PartitionsListManager partitionsManager)
        {
            _partitionsManager = partitionsManager;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<MechReactorComponent>()
                .With<PartitionsRootTag>()
                .Build();

            _charges = World.GetStash<EnergyChargeComponent>();
            _energyCellsGrid = World.GetStash<EnergyCellsGridComponent>();
            _nextEnergyCell = World.GetStash<NextEnergyCellComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var mechEntity in _filter)
            {
                var charge = 0f;
                var maxCharge = 0f;
                foreach (var partitionKvp in _partitionsManager.GetPartitionsList(mechEntity))
                {
                    var firstCell = _energyCellsGrid.Get(partitionKvp.Value).FirstCellEntity;
                    foreach (var cellEntity in new EnergyCellsEnumerator(_nextEnergyCell, firstCell))
                    {
                        var chargeComponent = _charges.Get(cellEntity);
                        charge += chargeComponent.Value;
                        maxCharge += chargeComponent.MaxValue;
                    }
                }

                _charges.Set(mechEntity, new(maxCharge, charge));
            }
        }

        public void Dispose() { }
    }
}