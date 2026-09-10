using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class EnergySystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            AdrenalineSubfeatureInstaller.AddSystems(installer);

            installer.AddSystem<EnergySystemsReceiveDamageSystem>(SystemGroupOrder.DamageApply1);

            installer.AddSystem<EnergyDistributionSystem>(SystemGroupOrder.EnergySystem);
            installer.AddSystem<EnergyChargingSystem>(SystemGroupOrder.EnergySystem);
        }
    }
}
