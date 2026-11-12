using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class VirtualViewFactory : IViewInstanceStrategy
    {
        // note: for complex views - continue develop virtual view containers

        private readonly World _world;
        private readonly VirtualViewApplier _virtualViewApplier;
        

        [Inject]
        public VirtualViewFactory(World world, VirtualViewApplier virtualViewApplier)
        {
            _world = world;
            _virtualViewApplier = virtualViewApplier;
        }

        Entity IViewInstanceStrategy.CreateViewEntity(string rawViewId)
        {
            // raw view id looks like: "virtual:VIEW_ID"
            var entity = _world.CreateEntity();
            _virtualViewApplier.ApplyRawViewId(entity, rawViewId);
            return entity;
        }

        public Entity CreateViewEntity(string viewConfigId)
        {
            var entity = _world.CreateEntity();
            _virtualViewApplier.Apply(entity, viewConfigId);
            return entity;
        }

        public Entity CreateViewEntity(VirtualViewConfigurationObject config)
        {
            var entity = _world.CreateEntity();
            _virtualViewApplier.Apply(entity, config.Data);
            return entity;
        }      
    }
}
