namespace ZE.MechBattle.GameStates
{
    public enum SceneStateKey : byte
    {
        Loading,
        Game,
        Fail
    }

    public class SceneStateMachine : GameStateMachineBase<SceneStateKey>
    {
        protected override SceneStateKey DefaultStateKey => SceneStateKey.Loading;

        protected override void PrepareAllStates()
        {
            AddState<SceneLoadingState>(SceneStateKey.Loading);
            AddState<SceneGameState>(SceneStateKey.Game);
            AddState<SceneFailState>(SceneStateKey.Fail);
        }
    }
}
