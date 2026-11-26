using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class ViewFactory
    {
        private readonly ViewApplier _viewApplier;
        private readonly World _world;
        private readonly ViewApplicationStrategiesList _viewStrategies;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;

        [Inject] 
        public ViewFactory(
            ViewApplier viewApplier, 
            World world, 
            ViewApplicationStrategiesList viewApplicationStrategies,
            ParentingRelationsApplier parentingRelationsApplier)
        {
            _viewApplier = viewApplier;
            _world = world;
            _viewStrategies = viewApplicationStrategies;
            _parentingRelationsApplier = parentingRelationsApplier;
        }
        
        public Entity CreateViewEntity(ViewKey viewKey)
        {
            var entity = _world.CreateEntity();
            _viewApplier.ApplyView(entity, viewKey);
            return entity;
        }
        
        public Entity CreateEmptyViewContainer(ViewMode viewType)
        {
            var entity = _world.CreateEntity();
            if (viewType != ViewMode.None)
                _viewStrategies[viewType].AddEmptyViewContainer(entity);
            return entity;

        }

        public Entity CreateChildEntityForViewPart(RigidTransform point, Entity parent, Entity viewOwner, ViewPartKey viewPartKey, ViewMode viewMode)
        {
            var entity = CreateEmptyViewContainer(viewMode);

            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = parent,
                ChildEntity = entity,

                ViewOwnerEntity = viewOwner,
                RequestedViewPartKey = viewPartKey,

                LocalPos = point.pos,
                LocalRot = point.rot,
            });
            return entity;
        }

    }
}
