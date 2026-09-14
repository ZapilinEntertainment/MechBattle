using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using ZE.Workers;

namespace ZE.MechBattle.States
{
    public abstract class GameStateMachineBase<T> : Worker, IInitializable, IStateSwitch<T>
        where T : Enum
    {
        protected abstract T DefaultStateKey { get; }
        private T _currentStateKey;
        private readonly Dictionary<T, IGameState> _states = new();

        public void Initialize()
        {
            PrepareAllStates();
            i_StartState(DefaultStateKey);
        }

        public override void Dispose()
        {
            base.Dispose();
            _states.Clear();
        }

        public void SwitchState(T nextStateKey)
        {
            _states[_currentStateKey].OnExit();
            i_StartState(nextStateKey);
        }

        protected abstract void PrepareAllStates();
       

        protected StateClass AddState<StateClass>(T stateKey) where StateClass : GameState<T>
        {
            var stateObject = AddSubWorker<StateClass>();
            stateObject.AssignSwitch(this);
            _states.Add(stateKey, stateObject);
            return stateObject;
        }

        private void i_StartState(T stateKey)
        {
            _currentStateKey = stateKey;
            _states[_currentStateKey].OnEnter();
        }
    }
}
