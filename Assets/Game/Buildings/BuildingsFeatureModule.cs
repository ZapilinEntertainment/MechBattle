using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class BuildingsFeatureModule : IFeatureModule, ISceneFeatureScopeInstaller
    {
        public void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SceneBuildingsInitializer>(Lifetime.Transient);
            builder.Register<BuildingsFactory>(Lifetime.Singleton);
        }
    }
}
