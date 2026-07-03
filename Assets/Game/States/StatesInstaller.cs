using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle.Ecs.States
{
    public class StatesInstaller : IFeatureInstaller
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
            AddState<PathfindingMoveState>(BehaviourKey.Tank, StateKey.Move);

            return dict;
        }

        public void InstallDependencies(IContainerBuilder builder)
        {
            builder.Register<DefaultIdleState>(Lifetime.Transient);
            builder.Register<DefaultMoveState>(Lifetime.Transient);
            builder.Register<PathfindingMoveState>(Lifetime.Transient);
        }

        public void Initialize(IObjectResolver resolver)
        {
           
        }
    }
}
