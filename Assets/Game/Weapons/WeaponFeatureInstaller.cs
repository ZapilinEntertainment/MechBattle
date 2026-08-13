using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class WeaponFeatureInstaller : EcsFeatureModule<WeaponSystemsInstallQueue>
    {

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<WeaponFactory>(Lifetime.Scoped);
        }

        protected override WeaponSystemsInstallQueue CreateQueue() => new();
    }
}
