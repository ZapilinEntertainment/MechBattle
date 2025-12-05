using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle.Ecs.States
{
    public static class StatesInstaller
    {
        public static void RegisterStates(IContainerBuilder builder) 
        { 
            builder.Register<DefaultIdleState>(Lifetime.Transient);
            builder.Register<DefaultMoveState>(Lifetime.Transient);
        }
        
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

            return dict;
        }
    
    }
}
