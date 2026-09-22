using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MovementDensityMapUpdateSystem : PausableSystem 
    {
        private Filter _movementCellsFilter;
        private Stash<CellEntityComponent> _cellEntities;
        private Stash<CellMovementDensityComponent> _movementDensity;
        private Stash<CellMovementDataComponent> _movementData;

        private readonly IEntitiesNavigationMap _map;

        private const float OCCUPIED_CELL_DENSITY = 1f;
        private const float NEIGHBOUR_CELL_DENSITY = 0.5f;
        private const float RESERVED_CELL_DENSITY = 0.3f;

        public MovementDensityMapUpdateSystem(SceneFlagsManager flags, IEntitiesNavigationMap map) : base(flags)
        {
            _map = map;
        }

        public override void OnAwake() 
        {
            _movementCellsFilter = World.Filter
                .With<CellEntityComponent>()
                .With<CellMovementDataComponent>()
                .Build();

            _cellEntities = World.GetStash<CellEntityComponent>();
            _movementDensity = World.GetStash<CellMovementDensityComponent>();
            _movementData = World.GetStash<CellMovementDataComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            _movementDensity.RemoveAll();

            foreach (var cellEntity in _movementCellsFilter)
            {
                var tripos = _cellEntities.Get(cellEntity).Tripos;
                var movementData = _movementData.Get(cellEntity).Value;
                if (movementData.Priority == 0)
                {
                    _movementDensity.Set(cellEntity, new(OCCUPIED_CELL_DENSITY));

                    if (tripos.IsPeak)
                    {
                        var offset = new PeakNeighbourOffsets();
                        foreach (var neighbourPos in new TriangleNeighboursEnumerator<PeakNeighbourOffsets>(tripos, offset))
                        {
                            if (_map.TryGetEntity(neighbourPos, out var neighbourCellEntity))
                                UpdateCellDensity(neighbourCellEntity, NEIGHBOUR_CELL_DENSITY);
                        }
                    }
                    else
                    {
                        var offset = new ValleyNeighbourOffsets();
                        foreach (var neighbourPos in new TriangleNeighboursEnumerator<ValleyNeighbourOffsets>(tripos, offset))
                        {
                            if (_map.TryGetEntity(neighbourPos, out var neighbourCellEntity))
                                UpdateCellDensity(neighbourCellEntity, NEIGHBOUR_CELL_DENSITY);
                        }
                    }
                }
                else
                {
                    // note: entity's own reservation may affect
                   // _movementDensity.Set(cellEntity, new(RESERVED_CELL_DENSITY));
                }
                
            }
        }

        private void UpdateCellDensity(Entity cellEntity, float newValue)
        {
            ref var densityComponent = ref _movementDensity.Get(cellEntity, out var exists);
            if (exists)
                densityComponent.Value = math.max(densityComponent.Value, newValue);
            else
                _movementDensity.Set(cellEntity, new(newValue));
        }
    }
}