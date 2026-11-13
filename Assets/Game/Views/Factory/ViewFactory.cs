using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class ViewFactory
    {
        private readonly ViewApplier _viewApplier;
        private readonly World _world;
        private readonly ViewApplicationStrategiesList _viewStrategies;

        [Inject] 
        public ViewFactory(ViewApplier viewApplier, World world, ViewApplicationStrategiesList viewApplicationStrategies)
        {
            _viewApplier = viewApplier;
            _world = world;
            _viewStrategies = viewApplicationStrategies;
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
            _viewStrategies[viewType].AddEmptyViewContainer(entity);
            return entity;

        }
        
    }
}
