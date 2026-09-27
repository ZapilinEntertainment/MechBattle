using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class NavigationCellEntitiesFactory
    {
        private readonly World _world;
        private readonly Stash<CellEntityComponent> _cells;
        private readonly Stash<CellPassabilityComponent> _cellPassabilities;
        private readonly Stash<CellMovementDensityComponent> _movementDensity;

        [Inject]
        public NavigationCellEntitiesFactory(World world)
        {
            _world = world;

            _cells = _world.GetStash<CellEntityComponent>();
            _cellPassabilities = _world.GetStash<CellPassabilityComponent>();
            _movementDensity = _world.GetStash<CellMovementDensityComponent>();
        }

        public Entity Build(IntTriangularPos tripos)
        {
            var entity = _world.CreateEntity();
            _cells.Add(entity, new(tripos));
            _cellPassabilities.Add(entity);
            _movementDensity.Add(entity);
            return entity;
        }
    
    }
}
