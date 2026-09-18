using System;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;
using R3;
using ZE.MechBattle.GameStates;

namespace ZE.MechBattle
{
    public class SessionGameState : GameState<SessionStateKey>
    {
        private SessionAsyncEntryPoint _sceneEntryPoint;
        private CancellationTokenSource _cancellationTokenSource = new();
        private LifetimeScope _sceneScope;
        private IDisposable _activeSubscription;

        [Inject]
        public SessionGameState(SessionAsyncEntryPoint entryPoint)
        {
            _sceneEntryPoint = entryPoint;
        }

        public override async void OnEnter()
        {
            await InitNewSceneScope();
        }

        public override void OnExit()
        {
            ClearActiveSubscriptions();
            _sceneScope.Dispose();
        }

        public override void Dispose()
        {
            base.Dispose();
            ClearActiveSubscriptions();
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private void ClearActiveSubscriptions()
        {
            if (_activeSubscription == null)
                return;

            _activeSubscription.Dispose();
            _activeSubscription = null;
        }

        private void OnSceneStateMachineStatusChanged(StateMachineStatus status)
        {
            if (status == StateMachineStatus.RestartRequired)
            {
                ClearActiveSubscriptions();
                ReloadScene();
                return;
            }

            if (status == StateMachineStatus.ExitRequired)
            {
                throw new System.NotImplementedException();
                // switch to menu state
            }
                
        }


        private async Awaitable InitNewSceneScope()
        {
            var token = _cancellationTokenSource.Token;
            await _sceneEntryPoint.StartAsync(token);
            _sceneScope = _sceneEntryPoint.SceneScope;

            var sceneStateMachine = _sceneScope.Container.Resolve<IContainerScopeStateMachine>();
            _activeSubscription = sceneStateMachine.StateMachineStatusProperty.Subscribe(OnSceneStateMachineStatusChanged);
        }

        private async Awaitable ReloadScene()
        {
            var scene = _sceneScope.gameObject.scene;
            var sceneIndex = scene.buildIndex;
            _sceneScope.Dispose();
            await Awaitable.NextFrameAsync();

            var unloadingOperation = SceneManager.UnloadSceneAsync(scene);
            while (!unloadingOperation.isDone)
                await Awaitable.NextFrameAsync();

           // UnityEngine.Debug.Log("scene unloaded");

            var loadingOperation = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
            while (!loadingOperation.isDone)
                await Awaitable.NextFrameAsync();

            var activeScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            SceneManager.SetActiveScene(activeScene);

           // UnityEngine.Debug.Log("scene loaded");

            OnEnter();
        }
    }
}
