using System.Collections.Generic;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using VContainer.Unity;

namespace ZE.MechBattle
{
    public class MorpehInstaller : IFeatureInstaller
    {
        private readonly List<IFeatureInstaller> _localInstallers = new()
        {
            new BaseEcsSystemsInstallQueue(),
            new MovementSystemsInstaller(),
            new StatesInstaller()            
        };

        public void PreloadResources(IObjectResolver globalContainerResolver) { }

        public void InstallDependencies(IContainerBuilder builder)
        {
            builder.Register<World>(_ => CreateWorld(), Lifetime.Scoped);

            builder.Register<ProjectileRequestsFactory>(Lifetime.Scoped);
            builder.Register<ProjectilesFactory>(Lifetime.Scoped);
            builder.Register<MonoViewFactory>(Lifetime.Scoped);
            builder.Register<ExplosionRequestsBuilder>(Lifetime.Scoped);
            builder.Register<DamageRequestsBuilder>(Lifetime.Scoped);
            builder.Register<VfxRequestsFactory>(Lifetime.Scoped);

            builder.Register<DelayApplier>(Lifetime.Scoped);
            builder.Register<TriangularPositionApplier>(Lifetime.Scoped);
            builder.Register<MoveTargetApplier>(Lifetime.Scoped);
            builder.Register<DisposeTagApplier>(Lifetime.Scoped);
            builder.Register<ParentingRelationsApplier>(Lifetime.Scoped);
            builder.Register<ViewSynchronizationApplier>(Lifetime.Scoped);
            builder.Register<ColliderOwnityApplier>(Lifetime.Scoped);

            builder.Register<MorpehSystemInstallHandler>(Lifetime.Scoped);

            foreach (var installer in _localInstallers)
                installer.InstallDependencies(builder);



            builder.RegisterEntryPoint<DamageablesInitializer>();
        }

        public void Initialize(IObjectResolver resolver)
        {
            foreach (var installer in _localInstallers)
                installer.Initialize(resolver);            
        }

        public void PostInitialize(IObjectResolver resolver)
        {
            var handler = resolver.Resolve<MorpehSystemInstallHandler>();
            handler.ApplySystems();
        }

        private static World CreateWorld()
        {
            var world = World.Create();
            // NOTE: NECESSARY!
            world.UpdateByUnity = true;
            //UnityEngine.Debug.Log($"registered: {world.GetHashCode()}");
            return world;
        }       
    }
}
