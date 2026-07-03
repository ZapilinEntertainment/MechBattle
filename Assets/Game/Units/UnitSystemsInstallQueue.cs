namespace ZE.MechBattle.Ecs
{
    public class UnitSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<SpawnersUpdateSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<UnitsCreationSystem>(SystemGroupOrder.RegularUpdate);
        }
    }
}
