using R3;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

namespace ZE.MechBattle.States
{
    public class SceneFailState : GameState<SceneStateKey>
    {
        private UIFailWindowWorker _worker;
        private IDisposable _subscription;

        public override void OnEnter() 
        {
            _worker = AddSubWorker<UIFailWindowWorker>();
            _subscription = _worker.FailOptionSelectedProperty.Subscribe(OnOptionSelected);
            _worker.Start();
        }

        public override void OnExit() 
        {
            ClearSubscription();
            _worker.Dispose();
            _worker = null;
        }

        private void OnOptionSelected(GameFailOption option)
        {
            ClearSubscription();
            if (option == GameFailOption.Restart)
            {
                UnityEngine.Debug.Log("restart game");
                ReloadScene();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ClearSubscription()
        {
            if (_subscription == null)
                return;
            _subscription.Dispose();
            _subscription = null;
        }

        private async Awaitable ReloadScene()
        {
            var sceneScope = ObjectResolver.Resolve<SceneScope>();
            sceneScope.Dispose();
            var activeSceneIndex = SceneManager.GetActiveScene().buildIndex;

            var unloadingOperation = SceneManager.UnloadSceneAsync(activeSceneIndex);
            while (!unloadingOperation.isDone)
                await Awaitable.NextFrameAsync();

            var loadingOperation = SceneManager.LoadSceneAsync(activeSceneIndex);
            while (!unloadingOperation.isDone)
                await Awaitable.NextFrameAsync();

            SwitchState(SceneStateKey.Loading);
        }
    }
}
