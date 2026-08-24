using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class CollidersFeatureInstaller : EcsFeatureModule<ColliderSystemsInstaller>, IAppFeatureScopeInstaller, ISceneFeatureScopeInstaller
    {
        protected override ColliderSystemsInstaller CreateQueue() => new();

        void IAppFeatureScopeInstaller.AppScopeInstall(IContainerBuilder builder)
        {
            builder.RegisterComponentOnNewGameObject<CollidersPool>(Lifetime.Singleton);
        }

        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<ColliderOwnityApplier>(Lifetime.Scoped);
            builder.Register<CollidersTable>(Lifetime.Scoped);
            builder.Register<CollidersFactory>(Lifetime.Scoped);
            builder.Register<ViewPartConnectionsList>(Lifetime.Scoped);
        }
    }
}
