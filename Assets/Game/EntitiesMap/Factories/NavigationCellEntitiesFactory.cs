using Scellecs.Morpeh;
using System.Collections.Generic;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class NavigationCellEntitiesFactory : NavigationDataEntitiesFactoryBase<INavigationCellCreationApplier>
    {
        private Stash<CellEntityComponent> _cells;
        private Stash<CellPassabilityComponent> _cellPassabilities;       
        private Stash<CellHeightComponent> _heights;

        public NavigationCellEntitiesFactory(World world, IEnumerable<INavigationCellCreationApplier> creationAppliers) : base(world, creationAppliers)
        {
        }

        protected override void Init()
        {
            _cells = _world.GetStash<CellEntityComponent>();
            _cellPassabilities = _world.GetStash<CellPassabilityComponent>();
            _heights = _world.GetStash<CellHeightComponent>();
        }

        public Entity Build(IntTriangularPos tripos)
        {
            // components existing are required for proper jobs functional (native stashes cannot have structural changes)
            var entity = base.BuildWithAppliers();
            _cells.Add(entity, new(tripos));
            _cellPassabilities.Add(entity);
            _heights.Add(entity);

            return entity;
        }
    
    }
}
