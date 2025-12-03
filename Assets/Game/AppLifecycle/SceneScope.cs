using VContainer.Unity;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class SceneScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            MorpehInstaller.SceneScopeInstall(builder);

            builder.Register<SessionData>(Lifetime.Scoped); 
            builder.Register<TransformAccessManager>(Lifetime.Scoped);
            
            builder.Register<MechBuilder>(Lifetime.Scoped);
            builder.Register<PlayerFactory>(Lifetime.Scoped);
            builder.Register<SceneFlagsManager>(Lifetime.Scoped);

            builder.Register<LoadingProcessesTable>(Lifetime.Scoped);
            builder.Register<RestorablesList>(Lifetime.Scoped);
            builder.Register<ViewReceiversList>(Lifetime.Scoped);
            builder.Register<CollidersTable>(Lifetime.Scoped);            

            builder.RegisterEntryPoint<SceneBootstrap>();
        }
    }
}
