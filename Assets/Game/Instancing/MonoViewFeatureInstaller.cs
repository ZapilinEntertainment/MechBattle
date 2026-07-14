using UnityEngine;
using VContainer;
using ZE.MechBattle.Views;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MonoViewFeatureInstaller : IFeatureInstaller
    {
        private MonoViewFeatureSystemsQueue _systemsQueue = new();

        public void InstallDependencies(IContainerBuilder builder)
        {
            _systemsQueue.InstallDependencies(builder);
        }

        public void Initialize(IObjectResolver resolver)
        {
            _systemsQueue.Initialize(resolver);

            var world = resolver.Resolve<World>();
            world.GetStash<DisposableViewComponent>().AsDisposable();
        }

        public void PostInitialize(IObjectResolver resolver) { }

        public void PreloadResources(IObjectResolver globalContainerResolver) { }
    }
}
