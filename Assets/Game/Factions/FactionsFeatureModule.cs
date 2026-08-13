using VContainer;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class FactionsFeatureModule : IFeatureModule, ISceneFeatureScopeInstaller
    {
        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            // todo: rework to discrete factions system
            builder.Register<PlayerRelations>(Lifetime.Scoped);
        }
    }
}
