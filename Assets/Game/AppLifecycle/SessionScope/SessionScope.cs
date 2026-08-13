using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace ZE.MechBattle
{
    public class SessionScope : FeaturedScopeBase<ISessionFeatureScopeInstaller, ISessionFeatureInitializer, ISessionFeaturePostInitializer>
    {

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterEntryPoint<SessionAsyncEntryPoint>();
            gameObject.name = nameof(SessionScope);

            UnityEngine.Debug.Log("session scope configured");
        }

        protected override void FeatureInitialize(ISessionFeatureInitializer initializer, IObjectResolver resolver) =>
            initializer.OnSessionContainerBuilt(resolver);

        protected override void FeaturePostInitialize(ISessionFeaturePostInitializer postInitializer, IObjectResolver resolver) =>
            postInitializer.OnSessionContainerPostBuilt(resolver);

        protected override void Install(ISessionFeatureScopeInstaller installer, IContainerBuilder containerBuilder) =>
            installer.SessionScopeInstall(containerBuilder);
    }
}
