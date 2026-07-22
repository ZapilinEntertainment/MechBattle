using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace ZE.MechBattle
{
    // load resources for child - SessionScope
    public class AppAsyncEntryPoint : AsyncScopeEntryPoint<ISessionAsyncResourceLoader>
    {
        private readonly LifetimeScope _currentScope;
        public AppAsyncEntryPoint(FeaturesModulesList modulesList, LifetimeScope currentScope) : base(modulesList)
        {
            _currentScope = currentScope;
        }

        public override async Awaitable StartAsync(CancellationToken cancellation)
        {
            var binder = await LoadResourcesAsync(cancellation);
            _currentScope.CreateChild<SessionScope>(builder => binder.Register(builder));
        }

        protected override Awaitable<IResourceBinder> LoadResourcesAsync(ISessionAsyncResourceLoader resourceLoader) =>
            resourceLoader.LoadSessionResourcesAsync(_currentScope.Container);
    }

}

