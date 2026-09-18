using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle
{

    // load resources for child - SceneScope
    public class SessionAsyncEntryPoint : AsyncScopeEntryPoint<ISceneAsyncResourceLoader>
    {
        public LifetimeScope SceneScope { get; private set; }
        private readonly LifetimeScope _currentScope;
        private IResourceBinder _resourceBinder;
        public SessionAsyncEntryPoint(FeaturesModulesList modulesList, LifetimeScope currentScope) : base(modulesList)
        {
            _currentScope = currentScope;
        }

        public override async Awaitable StartAsync(CancellationToken cancellation)
        {
            if (_resourceBinder == null)
                _resourceBinder = await LoadResourcesAsync(cancellation);

            SceneScope = GameObject.FindAnyObjectByType<SceneScope>(FindObjectsInactive.Exclude);  
            if (SceneScope == null)
            {
                Debug.LogWarning("scene scope object not found, building anew...");
                SceneScope = _currentScope.CreateChild<SceneScope>(builder => _resourceBinder.Register(builder));
            }
            else
            {
                SceneScope.Build();
            }
        }

        protected override Awaitable<IResourceBinder> LoadResourcesAsync(ISceneAsyncResourceLoader resourceLoader) =>
            resourceLoader.LoadSceneResourcesAsync(_currentScope.Container);
    }
}

