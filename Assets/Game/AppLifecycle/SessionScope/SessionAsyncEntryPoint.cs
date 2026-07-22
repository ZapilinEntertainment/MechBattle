using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle
{
    // load resources for child - SceneScope
    public class SessionAsyncEntryPoint : AsyncScopeEntryPoint<ISceneAsyncResourceLoader>
    {
        private readonly LifetimeScope _currentScope;
        public SessionAsyncEntryPoint(FeaturesModulesList modulesList, LifetimeScope currentScope) : base(modulesList)
        {
            _currentScope = currentScope;
        }

        public override async Awaitable StartAsync(CancellationToken cancellation)
        {
            var binder = await LoadResourcesAsync(cancellation);
            var sceneScope = GameObject.FindAnyObjectByType<SceneScope>(FindObjectsInactive.Exclude);  
            if (sceneScope == null)
            {
                Debug.LogWarning("scene scope object not found, building anew...");
                _currentScope.CreateChild<SceneScope>(builder => binder.Register(builder));
            }
            else
            {
                sceneScope.Build();
            }
        }

        protected override Awaitable<IResourceBinder> LoadResourcesAsync(ISceneAsyncResourceLoader resourceLoader) =>
            resourceLoader.LoadSceneResourcesAsync(_currentScope.Container);
    }
}

