using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class MonoViewFeatureModule : EcsFeatureModule<MonoViewFeatureSystemsQueue>, ISceneFeatureInitializer
    {
        protected override MonoViewFeatureSystemsQueue CreateQueue() => new();

        public override void OnSceneContainerBuilt(IObjectResolver resolver)
        {
            base.OnSceneContainerBuilt(resolver);
            var world = resolver.Resolve<World>();
            world.GetStash<DisposableViewComponent>().AsDisposable();
        }

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<FinalViewFunctionalApplier>(Lifetime.Scoped);
            builder.Register<EntityViewHandler>(Lifetime.Scoped);
            builder.Register<ViewPartsConnectionHandler>(Lifetime.Scoped);
        }

        public override void OnWorldDispose(World world, IObjectResolver resolver)
        {
            var filter = world.Filter.With<ViewContainerComponent>().Build();
            if (filter.IsEmpty())
                return;

            var viewContainersList = resolver.Resolve<ViewContainersPool>();
            var viewContainerStash = world.GetStash<ViewContainerComponent>();
            foreach (var entity in filter)
            {
                var viewContainer = viewContainerStash.Get(entity);
                viewContainersList.Release(viewContainer.Id);
            }

        }
    }
}
