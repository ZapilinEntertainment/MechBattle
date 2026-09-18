namespace ZE.MechBattle.GameStates
{
    public enum SessionStateKey : byte { Game}

    public class SessionStateMachine : GameStateMachineBase<SessionStateKey>
    {
        protected override SessionStateKey DefaultStateKey => SessionStateKey.Game;

        protected override void PrepareAllStates()
        {
            AddState<SessionGameState>(SessionStateKey.Game);
        }
    }
}
