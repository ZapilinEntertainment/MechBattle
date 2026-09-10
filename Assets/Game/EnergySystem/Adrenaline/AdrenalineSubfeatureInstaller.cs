using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public static class AdrenalineSubfeatureInstaller
    {
        public static void AddSystems(FeatureSystemsInstallQueue.ISystemsOperator installer)
        {
            installer.AddSystem<AdrenalineApplySystem>(SystemGroupOrder.EarlyUpdate);

            installer.AddSystem<DamageAdrenalineCalculationSystem>(SystemGroupOrder.AfterDamageCalculation);
            installer.AddSystem<EnergySpentAdrenalineCalculationSystem>(SystemGroupOrder.EnergySystem);
            installer.AddSystem<AdrenalineLevelUpdateSystem>(SystemGroupOrder.EnergySystem + 1);
            
        }
    
    }
}
