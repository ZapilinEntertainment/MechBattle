using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.PlayerData;
using ZE.MechBattle.UI;
using ZE.Workers;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class PlayerFeatureModule : EcsFeatureModule<PlayerSystemsInstallQueue>, ISceneFeatureScopeInstaller, ISceneFeatureInitializer, IAppFeatureScopeInstaller
    {
        void IAppFeatureScopeInstaller.AppScopeInstall(IContainerBuilder builder)
        {
            builder.Register<LocalPlayerInitializer>(Lifetime.Transient);
            builder.Register<PlayerUiInitializer>(Lifetime.Transient);
        }

        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<IPlayersList, PlayersList>(Lifetime.Scoped);
            builder.Register<PlayerHandler>(Lifetime.Scoped);
            builder.Register<PlayerFactory>(Lifetime.Scoped);
            
            RegisterWorker<CursorAimTrackingWorker>(builder);
            RegisterWorker<PlayerInterfaceWorker>(builder);
        }

        void ISceneFeatureInitializer.OnSceneContainerBuilt(IObjectResolver resolver)
        {
            base.OnSceneContainerBuilt(resolver);
            var playerFactory = resolver.Resolve<PlayerFactory>();
            var levelSettings = resolver.Resolve<LevelSettingsObject>();

            var localPlayerSpawn = levelSettings.GetSpawnPoint(GameConstants.LOCAL_PLAYER_KEY);
            var localPlayerEntity = playerFactory.CreateLocalPlayer(localPlayerSpawn.ToRigidTransform());

            var playerInitializer = resolver.Resolve<LocalPlayerInitializer>();
            playerInitializer.StartTracking(localPlayerEntity);
        }

        protected override PlayerSystemsInstallQueue CreateQueue() => new();


        // IMPORTANT: do not use AsImplementedInterfaces() for ITickable - it will double every instance on resolve
        void RegisterWorker<T>(IContainerBuilder builder) where T : Worker => builder.Register<T>(Lifetime.Transient);
    }
}
