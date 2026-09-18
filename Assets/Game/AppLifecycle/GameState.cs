using System;
using ZE.Workers;

namespace ZE.MechBattle.GameStates
{
    public interface IGameState : IDisposable
    {
        public void OnEnter();
        public void OnExit();
    }

    public abstract class GameState<T> : Worker, IGameState where T : Enum
    {
        protected IStateSwitch<T> _stateSwitch;

        public void AssignSwitch(IStateSwitch<T> stateSwitch) => _stateSwitch = stateSwitch;
        public abstract void OnEnter();
        public abstract void OnExit();

        protected void SwitchState(T key) => _stateSwitch.SwitchState(key);
        protected void RequestStateMachineStatusChange(StateMachineStatus status) => _stateSwitch.ChangeStateMachineStatus(status);
    }
}
