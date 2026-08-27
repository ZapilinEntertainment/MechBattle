using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle
{
    public class MechSystemsQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<MechInstanceSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystem<MechInitializationSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystem<MechInputSyncSystem>(SystemGroupOrder.MechSystemsCalculation);

            installer.AddSystem<MechIdleStandReturnCheckSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystem<MechMovementPrepareSystem>(SystemGroupOrder.MechSystemsCalculation);
            installer.AddSystemWithInterface<NextStepCellsCalculationSystem, IMechStepsAffectionMapSource>(SystemGroupOrder.MechSystemsCalculation);

            // calculating next position job -> switch to next systems group
            installer.AddSystem<MechStepsMapUpdateSystem>(SystemGroupOrder.MechSystemsApplication);

            installer.AddSystem<TargetStepPositionCheckSystem>(SystemGroupOrder.MechSystemsApplication);
            installer.AddSystem<StartChassisMovementSystem>(SystemGroupOrder.MechSystemsApplication);
            installer.AddSystem<StepProgressionSystem>(SystemGroupOrder.MechSystemsApplication);
            installer.AddSystem<StepInterpolationSystem>(SystemGroupOrder.MechSystemsApplication);
            installer.AddSystem<MechMovementClearSystem>(SystemGroupOrder.MechSystemsApplication);

            installer.AddSystem<TramplingSystem>(SystemGroupOrder.DamageApply - 1);

            installer.AddSystem<PartitionsClearSystem>(SystemGroupOrder.DisposedObjectsOperations);
            installer.AddSystem<MechStepsMapClearSystem>(SystemGroupOrder.Dispose);
        }
    }
}
