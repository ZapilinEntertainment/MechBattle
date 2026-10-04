using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Units.Squads;

namespace ZE.MechBattle
{
    public static class SquadSubfeatureInstaller
    {
        private class SquadStatesInstaller : FeatureStateInstallerBase
        {
            public SquadStatesInstaller()
            {
                States = new IEntityStateInstaller[4]
                {
                    new StateInstaller<SquadIdleState>(BehaviourKey.Squad, StateKey.Idle),
                    new StateInstaller<SquadMoveState>(BehaviourKey.Squad, StateKey.Move),
                    new StateInstaller<SquadAttackState>(BehaviourKey.Squad, StateKey.Attack),
                    new StateInstaller<SquadGuardState>(BehaviourKey.Squad, StateKey.Guard)
                };
            }
        }

        private static readonly SquadStatesInstaller _statesInstaller = new();

        public static void InstallSceneScopeDependencies(IContainerBuilder builder)
        {
            builder.Register<SquadFactory>(Lifetime.Singleton);
            builder.Register<SquadHandler>(Lifetime.Singleton);
            builder.Register<SquadDecreeApplier>(Lifetime.Singleton);

            builder.Register<SquadIdleState>(Lifetime.Transient);
            builder.Register<SquadMoveState>(Lifetime.Transient);
            builder.Register<SquadAttackState>(Lifetime.Transient);
            builder.Register<SquadGuardState>(Lifetime.Transient);
        }

        public static void InstallSystems(FeatureSystemsInstallQueue.ISystemsOperator installer)
        {
            installer.AddSystem<SquadUpdateSystem>(SystemGroupOrder.SquadUpdates);
            installer.AddLateSystem<SquadPositionCalculationSystem>(TriangularPosUpdateSystem.GroupOrder - 1);


            installer.AddSystem<SquadLossesUpdateSystem>(SystemGroupOrder.DisposedObjectsOperations);
            installer.AddSystem<SquadUpdateTagClearSystem>(SystemGroupOrder.Dispose);
        }

        public static void BindStates(IContainerBuilder builder) => _statesInstaller.BindStates(builder);

        public static void AddStatesToDictionary(StateBehavioursDictionary dictionary) => _statesInstaller.AddStatesToDictionary(dictionary);
    }
}
