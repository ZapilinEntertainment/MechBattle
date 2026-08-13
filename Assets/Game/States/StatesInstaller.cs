using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle.Ecs.States
{
    [System.Serializable]
    public class StatesInstaller : IFeatureModule, ISceneFeatureScopeInstaller, ISceneFeatureInitializer
    { 
        public static Dictionary<StateUpdateSystem.StateHandlerKey, StateHandler> PrepareStatesList(IObjectResolver resolver)
        {
            var dict = new Dictionary<StateUpdateSystem.StateHandlerKey, StateHandler>();

            void AddStateInstance<T>(BehaviourKey behaviour, StateKey state, T stateHandler) where T : StateHandler
            {
                dict.Add(new(behaviour, state), stateHandler);
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

            return dict;
        }

        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<StatesApplier>(Lifetime.Scoped);

            builder.Register<DefaultIdleState>(Lifetime.Transient);
            builder.Register<DefaultMoveState>(Lifetime.Transient);
            builder.Register<DefaultAttackState>(Lifetime.Transient);

            builder.Register<StateUpdateSystem>(Lifetime.Scoped);
        }

        void ISceneFeatureInitializer.OnSceneContainerBuilt(IObjectResolver resolver)
        {
            var systemsResolver = resolver.Resolve<MorpehSystemInstallHandler>();
            systemsResolver.AddSystem<StateUpdateSystem>(SystemGroupOrder.RegularUpdate);
        }
    }
}
