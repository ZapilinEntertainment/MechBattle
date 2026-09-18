using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle.Ecs
{
    public interface IEcsFeatureModule
    {
        void OnWorldDispose(World world, IObjectResolver resolver);
    }

    [System.Serializable]
    public abstract class EcsFeatureModule<InstallQueueType> : IFeatureModule, ISceneFeatureScopeInstaller, ISceneFeatureInitializer
        where InstallQueueType : FeatureSystemsInstallQueue
    {
        private readonly InstallQueueType _queueInstaller;
        protected abstract InstallQueueType CreateQueue();

        public EcsFeatureModule()
        {
            _queueInstaller = CreateQueue();
        }

        public virtual void SceneScopeInstall(IContainerBuilder builder)
        {
            _queueInstaller.InstallDependencies(builder);            
        }

        public virtual void OnSceneContainerBuilt(IObjectResolver resolver)
        {
            _queueInstaller.Initialize(resolver);
        }

        public virtual void OnWorldDispose(World world, IObjectResolver resolver) { }
    }
}
