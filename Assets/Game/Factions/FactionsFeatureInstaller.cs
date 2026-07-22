using VContainer;

namespace ZE.MechBattle
{
    public class FactionsFeatureInstaller : IFeatureModule, ISceneFeatureScopeInstaller
    {
        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<FinalViewFunctionalApplier>(Lifetime.Scoped);
            builder.Register<PlayerRelations>(Lifetime.Scoped);
            builder.Register<IPlayersList, PlayersList>(Lifetime.Scoped);
        }
    }
}
