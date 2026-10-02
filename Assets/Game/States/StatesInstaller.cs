using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle.Ecs.States
{
    [System.Serializable]
    public class StatesInstaller : IFeatureModule, ISceneFeatureScopeInstaller, ISceneFeatureInitializer
    { 
        public static StateBehavioursDictionary PrepareStatesList(IObjectResolver resolver)
        {
            var dict = new StateBehavioursDictionary();

            void AddStateInstance<T>(BehaviourKey behaviour, StateKey state, T stateHandler) where T : StateHandler
            {
                dict.Add(StateHandlerKey.ToLong(state, behaviour), stateHandler);
            }

            T AddState<T>(BehaviourKey behaviour, StateKey state) where T : StateHandler
            {
                var instance = resolver.Resolve<T>();
                AddStateInstance(behaviour, state, instance);
                return instance;
            }
            

            AddState<DefaultIdleState>(BehaviourKey.Tank, StateKey.Idle);
            AddState<DefaultMoveState>(BehaviourKey.Tank, StateKey.Move);
            AddState<DefaultAttackState>(BehaviourKey.Tank, StateKey.Attack);

            AddState<SquadIdleState>(BehaviourKey.Squad, StateKey.Idle);
            AddState<SquadMoveState>(BehaviourKey.Squad, StateKey.Move);
            AddState<SquadAttackState>(BehaviourKey.Squad, StateKey.Attack);
            AddState<SquadGuardState>(BehaviourKey.Squad, StateKey.Guard);

            return dict;
        }

        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<StatesApplier>(Lifetime.Singleton);

            builder.Register<DefaultIdleState>(Lifetime.Transient);
            builder.Register<DefaultMoveState>(Lifetime.Transient);
            builder.Register<DefaultAttackState>(Lifetime.Transient);

            builder.Register<StateBehavioursDictionary>(resolver => StatesInstaller.PrepareStatesList(resolver), Lifetime.Singleton);

            builder.Register<StateUpdateSystem<RegularPriorityStateMachineTag>>(Lifetime.Transient);
            builder.Register<StateUpdateSystem<HighPriorityStateMachineTag>>(Lifetime.Transient);
        }

        void ISceneFeatureInitializer.OnSceneContainerBuilt(IObjectResolver resolver)
        {
            var systemsResolver = resolver.Resolve<MorpehSystemInstallHandler>();
            systemsResolver.AddSystem<StateUpdateSystem<HighPriorityStateMachineTag>>(SystemGroupOrder.RegularUpdate);
            systemsResolver.AddSystem<StateUpdateSystem<RegularPriorityStateMachineTag>>(SystemGroupOrder.RegularUpdate);
        }
    }
}
