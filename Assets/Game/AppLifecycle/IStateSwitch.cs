using System;
using ZE.MechBattle.GameStates;

namespace ZE.MechBattle
{
    public interface IStateSwitch<T> where T : Enum
    {
        void SwitchState(T stateKey);
        void ChangeStateMachineStatus(StateMachineStatus status);
    }
}
