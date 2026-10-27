using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.PlayerData;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class PlayerFeatureModule : EcsFeatureModule<PlayerSystemsInstallQueue>, ISceneFeatureScopeInstaller
    {

        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<IPlayersList, PlayersList>(Lifetime.Scoped);
            builder.Register<PlayerHandler>(Lifetime.Scoped);
            builder.Register<PlayerFactory>(Lifetime.Scoped);

            builder.Register<AffinityHandler>(Lifetime.Scoped);

            builder.Register<ICursorAimTracker,CursorAimTrackingWorker>(Lifetime.Scoped);
            builder.Register<MechHeadRotationWorker>(Lifetime.Scoped);

            builder.RegisterEntryPoint<LocalPlayerInitializer>(Lifetime.Transient);

            builder.Register<IBuildingFeatureApplier, BuildingPlayerAffinityApplier>(Lifetime.Singleton);
        }

        protected override PlayerSystemsInstallQueue CreateQueue() => new();


    }
}
