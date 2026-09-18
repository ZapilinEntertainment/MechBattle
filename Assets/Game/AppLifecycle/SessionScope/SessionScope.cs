using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.GameStates;

namespace ZE.MechBattle
{
    public class SessionScope : FeaturedScopeBase<ISessionFeatureScopeInstaller, ISessionFeatureInitializer, ISessionFeaturePostInitializer>
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            // entry point will be called by session state machine
            // multiple times on scene restart
            builder.Register<SessionAsyncEntryPoint>(Lifetime.Scoped);

            gameObject.name = nameof(SessionScope);

            InstallStates(builder);

#if UNITY_EDITOR
            UnityEngine.Debug.Log("session scope configured");
#endif
        }

        protected override void FeatureInitialize(ISessionFeatureInitializer initializer, IObjectResolver resolver) =>
            initializer.OnSessionContainerBuilt(resolver);

        protected override void FeaturePostInitialize(ISessionFeaturePostInitializer postInitializer, IObjectResolver resolver) =>
            postInitializer.OnSessionContainerPostBuilt(resolver);

        protected override void Install(ISessionFeatureScopeInstaller installer, IContainerBuilder containerBuilder) =>
            installer.SessionScopeInstall(containerBuilder);

        private void InstallStates(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SessionStateMachine>(Lifetime.Singleton);

            builder.Register<SessionGameState>(Lifetime.Transient);
        }
    }
}
