using R3;
using System;
using VContainer;

namespace ZE.MechBattle.GameStates
{
    public class SceneFailState : GameState<SceneStateKey>
    {
        private UIFailWindowWorker _worker;
        private CompositeDisposable _activeCompositeDisposable = new();
        private readonly SceneFlagsManager _sceneFlags;

        [Inject]
        public SceneFailState(SceneFlagsManager sceneFlagsManager)
        {
            _sceneFlags = sceneFlagsManager;
        }

        public override void OnEnter() 
        {
            _worker = AddSubWorker<UIFailWindowWorker>();
            _worker.Start();
            _worker.FailOptionSelectedProperty
                .Subscribe(OnOptionSelected)
                .AddTo(_activeCompositeDisposable);
            _sceneFlags
                .AddTemporalFlag<PauseFlag>()
                .AddTo(_activeCompositeDisposable);
        }

        public override void OnExit() 
        {
            _activeCompositeDisposable.Clear();
            _worker.Dispose();
            _worker = null;
        }

        private void OnOptionSelected(GameFailOption option)
        {
            _activeCompositeDisposable.Clear();
            if (option == GameFailOption.Restart)
            {
#if UNITY_EDITOR
                UnityEngine.Debug.Log("restart game");
#endif
                RequestStateMachineStatusChange(StateMachineStatus.RestartRequired);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _activeCompositeDisposable.Dispose();
        }
    }
}
