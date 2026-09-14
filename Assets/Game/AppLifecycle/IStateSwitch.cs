using System;

namespace ZE.MechBattle
{
    public interface IStateSwitch<T> where T : Enum
    {
        void SwitchState(T stateKey);    
    }
}
