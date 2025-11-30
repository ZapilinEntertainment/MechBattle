using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public static class MorpehInstaller
    {
          private enum SystemGroupOrder : byte { Initialization = 0, Default = 1, FixedUpdateGroup = 2}

        public static void AppScopeInstall(IContainerBuilder builder)
        {
            RegisterSystem<ViewRequestsHandleSystem>();            
            RegisterSystem<VfxCreateSystem>();
            RegisterSystem<RestorationSystem>();

            RegisterSystem<ProjectileCreateSystem>();
            RegisterSystem<ProjectileMoveSystem>();
            RegisterSystem<ProjectilesExplodeSystem>();
            
            void RegisterSystem<T>() where T : class, ISystem => builder.Register<T>(Lifetime.Transient);
        }    

        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<World>(_ => CreateWorld(), Lifetime.Scoped);

            // world injection required:
            builder.Register<ProjectileRequestsFactory>(Lifetime.Scoped);
            builder.Register<ProjectileBuilder>(Lifetime.Scoped);
            builder.Register<ProjectileViewBuilder>(Lifetime.Scoped);
            builder.Register<ExplosionRequestsBuilder>(Lifetime.Scoped);
            builder.Register<DamageRequestsBuilder>(Lifetime.Scoped);
            builder.Register<VfxRequestsBuilder>(Lifetime.Scoped);
        }
        private static World CreateWorld()
        {
            var world = World.Create();
            // NOTE: NECESSARY!
            world.UpdateByUnity = true;
            //UnityEngine.Debug.Log($"registered: {world.GetHashCode()}");
            return world;
        }

        public static void OnDependenciesResolved(IObjectResolver resolver)
        {            
            var world = resolver.Resolve<World>();
            //UnityEngine.Debug.Log($"resolved: {world.GetHashCode()}");

            var initGroup = world.CreateSystemsGroup();
            initGroup.AddInitializer(new Initializer());
            world.AddSystemsGroup((int)SystemGroupOrder.Initialization, initGroup);

            var defaultGroup = world.CreateSystemsGroup();
            AddSystem<ViewRequestsHandleSystem>(defaultGroup);
            AddSystem<ProjectileCreateSystem>(defaultGroup);            
            AddSystem<VfxCreateSystem>(defaultGroup);
            AddSystem<RestorationSystem>(defaultGroup);            
            world.AddSystemsGroup((int)SystemGroupOrder.Default, defaultGroup);

            var fixedUpdateGroup = world.CreateSystemsGroup();
            AddSystem<ProjectileMoveSystem>(fixedUpdateGroup);
            AddSystem<ProjectilesExplodeSystem>(fixedUpdateGroup);

            world.AddSystemsGroup((int)SystemGroupOrder.FixedUpdateGroup, fixedUpdateGroup);

            world.Commit();

            void AddSystem<T>(SystemsGroup group) where T: class,ISystem 
            {
                var system = resolver.Resolve<T>();
                group.AddSystem(system);
            }
        }
    }
}
