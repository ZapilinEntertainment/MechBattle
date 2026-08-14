using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.PlayerData;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class PlayerFeatureModule : EcsFeatureModule<PlayerSystemsInstallQueue>, ISceneFeatureScopeInstaller, ISceneFeatureInitializer, IAppFeatureScopeInstaller
    {
        void IAppFeatureScopeInstaller.AppScopeInstall(IContainerBuilder builder)
        {
            builder.Register<LocalPlayerInitializer>(Lifetime.Transient);
        }

        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<IPlayersList, PlayersList>(Lifetime.Scoped);
            builder.Register<PlayerHandler>(Lifetime.Scoped);
            builder.Register<PlayerFactory>(Lifetime.Scoped);            
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
    }
}
