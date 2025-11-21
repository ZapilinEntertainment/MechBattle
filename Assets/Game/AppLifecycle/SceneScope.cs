using VContainer.Unity;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{
    public class SceneScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<SessionData>(Lifetime.Scoped);
            builder.Register<World>(Lifetime.Scoped);    
            
            builder.Register<MechBuilder>(Lifetime.Scoped);
            builder.Register<PlayerFactory>(Lifetime.Scoped);

            builder.RegisterEntryPoint<SceneBootstrap>();
        }
    }
}
