using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Energy;

namespace ZE.MechBattle
{
    public class EnergySystemFeatureModule : EcsFeatureModule<EnergySystemsInstallQueue>
    {
        protected override EnergySystemsInstallQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);

            builder.Register<EnergyDamageApplier>(Lifetime.Scoped);
            builder.Register<EnergyCellsFactory>(Lifetime.Scoped);
        }
    }
}
