using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Navigation.Ecs
{
    public class CellEntitiesHandler
    {
        private readonly World _world;
        private readonly Stash<CellEntityComponent> _cellEntities;
        private readonly Stash<CellHeightComponent> _heights;
        private readonly Stash<CellPassabilityComponent> _passabilities;

        [Inject]
        public CellEntitiesHandler(World world)
        {
            _world = world;

            _cellEntities = _world.GetStash<CellEntityComponent>();
            _heights = _world.GetStash<CellHeightComponent>();
            _passabilities = _world.GetStash<CellPassabilityComponent>();
        }

        public Entity CreateCellEntity(IntTriangularPos tripos)
        {
            var entity = _world.CreateEntity();
            _cellEntities.Set(entity, new(tripos));
            return entity;
        }

        public void SetEntityPassability(Entity entity, CellPassabilityData passability) => _passabilities.Set(entity, new() { Value = passability });
        public void SetEntityHeight(Entity entity, CellHeightData heightData) => _heights.Set(entity, new() { Value = heightData });

        public CellHeightData GetHeightData(Entity cellEntity) => 
            GetComponentOrDefaultValueCommand
            .Execute(cellEntity, _heights, NavigationLogic.GetDefaultHeightData());

        public CellPassabilityData GetPassability(Entity cellEntity, bool defaultPassability) =>
            GetComponentOrDefaultValueCommand
            .Execute(cellEntity, _passabilities, NavigationLogic.GetDefaultPassability(defaultPassability));

    }
}
