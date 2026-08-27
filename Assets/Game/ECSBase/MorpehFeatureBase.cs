using Scellecs.Morpeh;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class MorpehFeatureBase : EcsFeatureModule<BaseEcsSystemsInstallQueue>, ISceneFeaturePostInitializer
    {
        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);

            builder.Register<World>(_ => CreateWorld(), Lifetime.Scoped);

            builder.Register<ProjectileRequestsFactory>(Lifetime.Scoped);
            builder.Register<ProjectilesFactory>(Lifetime.Scoped);
            builder.Register<MonoViewFactory>(Lifetime.Scoped);
            builder.Register<ExplosionRequestsBuilder>(Lifetime.Scoped);       

            builder.Register<DelayApplier>(Lifetime.Scoped);
            builder.Register<TriangularPositionApplier>(Lifetime.Scoped);
            builder.Register<MoveTargetApplier>(Lifetime.Scoped);
            builder.Register<DisposeTagApplier>(Lifetime.Scoped);
            builder.Register<ParentingRelationsApplier>(Lifetime.Scoped);
            builder.Register<ViewSynchronizationApplier>(Lifetime.Scoped);            

            builder.Register<MorpehSystemInstallHandler>(Lifetime.Scoped);
            builder.Register<LifetimeTrackingManager>(Lifetime.Scoped);            
        }

        void ISceneFeaturePostInitializer.OnSceneContainerPostBuilt(IObjectResolver resolver)
        {
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
