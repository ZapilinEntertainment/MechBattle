using Scellecs.Morpeh;
using System;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class MorpehFeatureBase : EcsFeatureModule<BaseEcsSystemsInstallQueue>, ISceneFeaturePostInitializer
    {
        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);

            builder.Register<WorldDisposer>(Lifetime.Transient).As<IDisposable>();
            builder.Register<World>(_ => CreateWorld(), Lifetime.Singleton);

            builder.Register<ProjectileRequestsFactory>(Lifetime.Singleton);
            builder.Register<ProjectilesFactory>(Lifetime.Singleton);
            builder.Register<MonoViewFactory>(Lifetime.Singleton);
            builder.Register<ExplosionRequestsBuilder>(Lifetime.Singleton);       

            builder.Register<DelayApplier>(Lifetime.Singleton);
            builder.Register<TriangularPositionApplier>(Lifetime.Singleton);
            builder.Register<MoveTargetApplier>(Lifetime.Singleton);
            builder.Register<DisposeTagApplier>(Lifetime.Singleton);
            builder.Register<ParentingRelationsApplier>(Lifetime.Singleton);
            builder.Register<ViewSynchronizationApplier>(Lifetime.Singleton);            

            builder.Register<MorpehSystemInstallHandler>(Lifetime.Singleton);
            builder.Register<LifetimeTrackingManager>(Lifetime.Singleton);

            builder.Register<WorkersFactory>(Lifetime.Singleton);

            builder.Register<CreationResultsList>(Lifetime.Singleton);
            builder.Register<CreationRequestsHandler>(Lifetime.Singleton);
           
        }

        void ISceneFeaturePostInitializer.OnSceneContainerPostBuilt(IObjectResolver resolver)
        {
            base.OnSceneContainerPostBuilt(resolver);
            var handler = resolver.Resolve<MorpehSystemInstallHandler>();
            handler.ApplySystems();
        }

        private World CreateWorld()
        {
            var world = World.Create();
            // NOTE: NECESSARY!
            world.UpdateByUnity = true;
            //UnityEngine.Debug.Log($"registered: {world.GetHashCode()}");
            return world;
        }

        protected override BaseEcsSystemsInstallQueue CreateQueue() => new();

        
    }
}
