using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechSystemsQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<MechInstanceSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystem<StepDataSetupSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystemWithInterface<NextStepCellsCalculationSystem, IMechStepsAffectionMap>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystem<MechStepApplicationSystem>(SystemGroupOrder.MechSystemsApplication);
        }
    }
}
