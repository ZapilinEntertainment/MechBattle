using R3;
using ZE.MechBattle.GameStates;

namespace ZE.MechBattle
{
    public interface IContainerScopeStateMachine
    {
        public Observable<StateMachineStatus> StateMachineStatusProperty { get; }
    
    }
}
