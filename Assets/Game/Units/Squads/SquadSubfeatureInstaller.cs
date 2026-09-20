using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Units.Squads;

namespace ZE.MechBattle
{
    public static class SquadSubfeatureInstaller
    {
        public static void InstallSceneScopeDependencies(IContainerBuilder builder)
        {
            builder.Register<SquadFactory>(Lifetime.Singleton);
            builder.Register<SquadHandler>(Lifetime.Singleton);
            builder.Register<SquadsManager>(Lifetime.Singleton);
            builder.Register<SquadDecreeApplier>(Lifetime.Singleton);
            builder.Register<SquadMembersIterator>(Lifetime.Singleton);
        }

        public static void InstallSystems(FeatureSystemsInstallQueue.ISystemsOperator installer)
        {
            installer.AddSystem<SquadUpdateSystem>(SystemGroupOrder.SquadUpdates);
            installer.AddSystem<SquadDecreeDistributeSystem>(SystemGroupOrder.SquadUpdates);
            installer.AddSystem<SquadDecreesHandleSystem>(SystemGroupOrder.SquadUpdates);

            installer.AddSystem<SquadLossesUpdateSystem>(SystemGroupOrder.DisposedObjectsOperations);
            installer.AddSystem<SquadUpdateTagClearSystem>(SystemGroupOrder.Dispose);
        }
    
    }
}
