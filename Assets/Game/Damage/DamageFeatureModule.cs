using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Damage;

namespace ZE.MechBattle
{
    public class DamageFeatureModule : EcsFeatureModule<DamageSystemsInstallQueue>
    {
        protected override DamageSystemsInstallQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<DamageApplier>(Lifetime.Scoped);
            builder.Register<DamageRequestsList>(Lifetime.Scoped);
            builder.Register<ReceivedDamageList>(Lifetime.Scoped);            

            builder.RegisterEntryPoint<DamageablesInitializer>();
        }
    }
}
