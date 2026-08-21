using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.GridOperations;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class GridOperationsInstaller : EcsFeatureModule<GridSystemsInstallQueue>, ISceneFeatureScopeInstaller
    {
        protected override GridSystemsInstallQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);

            builder.Register<NavigationGridHandler>(Lifetime.Scoped);
            builder.Register<IUnitsGrid, UnitsGrid>(Lifetime.Scoped);
        }
    }
}
