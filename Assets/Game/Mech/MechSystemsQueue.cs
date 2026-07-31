using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class MechSystemsQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<MechInstanceSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystem<MechInitializationSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystem<MechMovementPrepareSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystemWithInterface<NextStepCellsCalculationSystem, IMechStepsAffectionMap>(SystemGroupOrder.MechSystemsCalculation);

            installer.AddSystem<TargetStepPositionCheckSystem>(SystemGroupOrder.MechSystemsApplication);
            installer.AddSystem<StartChassisMovementSystem>(SystemGroupOrder.MechSystemsApplication);
            installer.AddSystem<StepProgressionSystem>(SystemGroupOrder.MechSystemsApplication);
            installer.AddSystem<StepInterpolationSystem>(SystemGroupOrder.MechSystemsApplication);

        }
    }
}
