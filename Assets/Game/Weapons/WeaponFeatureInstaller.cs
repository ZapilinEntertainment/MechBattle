using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class WeaponFeatureInstaller : EcsFeatureInstaller<WeaponSystemsInstallQueue>
    {

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<WeaponFactory>(Lifetime.Scoped);
        }

        protected override WeaponSystemsInstallQueue CreateQueue() => new();
    }
}
