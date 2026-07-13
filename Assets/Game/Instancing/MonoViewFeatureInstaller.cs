using UnityEngine;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class MonoViewFeatureInstaller : IFeatureInstaller
    {
        private MonoViewFeatureSystemsQueue _systemsQueue = new();

        public void Initialize(IObjectResolver resolver)
        {
            _systemsQueue.Initialize(resolver);
        }

        public void InstallDependencies(IContainerBuilder builder)
        {
            _systemsQueue.InstallDependencies(builder);
        }

        public void PostInitialize(IObjectResolver resolver) { }

        public void PreloadResources(IObjectResolver globalContainerResolver) { }
    }
}
