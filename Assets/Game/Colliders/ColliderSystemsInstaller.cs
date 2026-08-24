using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class ColliderSystemsInstaller : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<CollidersRegistrationCeaseSystem>(SystemGroupOrder.DisposedObjectsOperations);
        }
    }
}
