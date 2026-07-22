using VContainer;

namespace ZE.MechBattle.Ecs
{
    public abstract class EcsFeatureInstaller<InstallQueueType> : IFeatureModule, ISceneFeatureScopeInstaller, ISceneFeatureInitializer
        where InstallQueueType : FeatureSystemsInstallQueue
    {
        private readonly InstallQueueType _queueInstaller;
        protected abstract InstallQueueType CreateQueue();

        public EcsFeatureInstaller()
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
    }
}
