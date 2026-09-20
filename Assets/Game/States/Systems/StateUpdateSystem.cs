using System;
using UnityEngine;
using VContainer;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class StateUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _updateTag;
        private Stash<BehaviourKeyComponent> _behaviourKeys;
        private Stash<StateComponent> _currentStates;
        private readonly Dictionary<long, StateHandler> _behaviours;
        private readonly IObjectResolver _resolver;

        [Inject]
        public StateUpdateSystem(IObjectResolver resolver)
        {
            _behaviours = StatesInstaller.PrepareStatesList(resolver);
        }

        public void OnAwake() 
        {
            _updateTag = World.Filter
                .With<BehaviourKeyComponent>()
                .With<StateComponent>()
                .Build();

            _behaviourKeys = World.GetStash<BehaviourKeyComponent>();
            _currentStates = World.GetStash<StateComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_updateTag.IsEmpty())
                return;

            var dt = Time.deltaTime;
            foreach (var entity in _updateTag)
            {
                ref var stateComponent = ref _currentStates.Get(entity);
                
                var behaviour = _behaviourKeys.Get(entity).Value;
                var state = _currentStates.Get(entity).CurrentState;
                var nextState = stateComponent.NextState;

                bool currentBehaviourExists;
                StateHandler currentBehaviour;
                if (nextState != state)
                {
                    if (TryGetStateBehaviour(StateHandlerKey.ToLong(state, behaviour), out var oldBehaviour))
                        oldBehaviour.Exit(entity);

                    currentBehaviourExists = TryGetStateBehaviour(StateHandlerKey.ToLong(nextState, behaviour), out currentBehaviour);
                    if (currentBehaviourExists)
                        currentBehaviour.Enter(entity);

                    stateComponent.CurrentState = nextState;
                }
                else
                {
                    currentBehaviourExists = TryGetStateBehaviour(StateHandlerKey.ToLong(state,behaviour), out currentBehaviour);
                }

               if (currentBehaviourExists)  stateComponent.NextState = currentBehaviour.Update(entity, dt);
            }
        }

        public void Dispose()
        {
            foreach (var behaviour in _behaviours.Values)
            {
                if (behaviour is IDisposable disposable)
                    disposable.Dispose();
            }
            _behaviours.Clear();
        }

        private bool TryGetStateBehaviour(long key, out StateHandler behaviour)
        {
            if (_behaviours.TryGetValue(key, out behaviour))
                return true;
            return false;
            //#if UNITY_EDITOR
            //Debug.LogWarning($"state behaviour not found: {key.Behaviour} : {key.State}");
            //return false;
            //#endif
        }
    }
}