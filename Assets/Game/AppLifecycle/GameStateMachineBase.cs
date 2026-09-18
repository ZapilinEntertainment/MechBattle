using R3;
using System;
using System.Collections.Generic;
using VContainer;
using VContainer.Unity;
using ZE.Workers;

namespace ZE.MechBattle.GameStates
{
    public enum StateMachineStatus : byte { NotStarted, Working, ExitRequired, RestartRequired}

    public abstract class GameStateMachineBase<T> : Worker, IInitializable, IStateSwitch<T>, IContainerScopeStateMachine
        where T : Enum
    {
        public Observable<StateMachineStatus> StateMachineStatusProperty => _statusReactiveProperty;
        protected abstract T DefaultStateKey { get; }
        private T _currentStateKey;
        private readonly Dictionary<T, IGameState> _states = new();
        private readonly ReactiveProperty<StateMachineStatus> _statusReactiveProperty = new();

        public virtual void Initialize()
        {
            PrepareAllStates();
            i_StartState(DefaultStateKey);
        }

        public override void Dispose()
        {
            base.Dispose();
            _states.Clear();
            _statusReactiveProperty.Dispose();
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

        public void ChangeStateMachineStatus(StateMachineStatus status) => _statusReactiveProperty.Value = status;
    }
}
