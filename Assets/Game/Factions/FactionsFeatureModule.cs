using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class FactionsFeatureModule : EcsFeatureModule<FactionSystemsInstallQueue>, ISceneFeatureScopeInstaller
    {
        protected override FactionSystemsInstallQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            // todo: rework to discrete factions system
            builder.Register<PlayerRelations>(Lifetime.Scoped);
            builder.Register<FactionHandler>(Lifetime.Singleton);
        }
    }
}
