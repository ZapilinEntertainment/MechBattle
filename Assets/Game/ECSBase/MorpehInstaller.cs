using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Ecs.Pathfinding;

namespace ZE.MechBattle
{
    public static class MorpehInstaller
    {
          private enum SystemGroupOrder : byte { Initialization = 0, Default = 1, FixedUpdateGroup , LateUpdateGroup, ClearSystems}

        public static void AppScopeInstall(IContainerBuilder builder)
        {
            RegisterInitializer<SceneInitializer>();
            RegisterInitializer<DamageablesInitializer>();
            RegisterInitializer<SceneUnitsInitializer>();

            RegisterSystem<ViewRequestsHandleSystem>();            
            RegisterSystem<VfxCreateSystem>();
            RegisterSystem<RestorationSystem>();

            RegisterSystem<ProjectileCreateSystem>();
            RegisterSystem<ProjectileMoveSystem>();
            RegisterSystem<ProjectilesExplodeSystem>();
            
            RegisterSystem<DamageCalculationSystem>();
            RegisterSystem<DamageApplySystem>();

            RegisterSystem<StateUpdateSystem>();
            RegisterSystem<TransformsSyncSystem>();

            RegisterSystem<ViewDestroyEffectSystem>();

            RegisterSystem<CollidersClearSystem>();
            RegisterSystem<EntityDisposeSystem>();
            RegisterSystem<UpdateTagsClearSystem>();
            RegisterSystem<TransformsClearSystem>();

            builder.Register<PathfinderFactory>(Lifetime.Scoped);
            builder.Register<PathsManager>(Lifetime.Scoped);
            RegisterSystem<PathRequestsHandleSystem>();
            RegisterSystem<PathUpdateSystem>();
            RegisterSystem<PathsClearSystem>();
            RegisterSystem<PathActualizingSystem>();

            StatesInstaller.RegisterStates(builder);

            void RegisterSystem<T>() where T : class, ISystem => builder.Register<T>(Lifetime.Transient);
            void RegisterInitializer<T>() where T : class, IInitializer => builder.Register<T>(Lifetime.Transient);
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
            builder.Register<EntityFactory>(Lifetime.Scoped);            
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

            void AddSystem<T>(SystemsGroup group) where T : class, ISystem
            {
                var system = resolver.Resolve<T>();
                group.AddSystem(system);
            }

            void AddInitializer<T>(SystemsGroup group) where T: class, IInitializer
            {
                var initializer = resolver.Resolve<T>();
                group.AddInitializer(initializer);
            }

            var initGroup = world.CreateSystemsGroup();
            AddInitializer<SceneInitializer>(initGroup);
            world.AddSystemsGroup((int)SystemGroupOrder.Initialization, initGroup);

            var defaultGroup = world.CreateSystemsGroup();
            AddInitializer<DamageablesInitializer>(defaultGroup);
            AddInitializer<SceneUnitsInitializer>(defaultGroup);
            AddSystem<ViewRequestsHandleSystem>(defaultGroup);
            AddSystem<StateUpdateSystem>(defaultGroup);
            AddSystem<ProjectileCreateSystem>(defaultGroup);     
            AddSystem<DamageCalculationSystem>(defaultGroup);
            AddSystem<DamageApplySystem>(defaultGroup);
            AddSystem<VfxCreateSystem>(defaultGroup);
            AddSystem<RestorationSystem>(defaultGroup);   

            AddSystem<PathActualizingSystem>(defaultGroup);
            AddSystem<PathRequestsHandleSystem>(defaultGroup);
            AddSystem<PathUpdateSystem>(defaultGroup);

            world.AddSystemsGroup((int)SystemGroupOrder.Default, defaultGroup);

            var fixedUpdateGroup = world.CreateSystemsGroup();
            AddSystem<ProjectileMoveSystem>(fixedUpdateGroup);
            AddSystem<ProjectilesExplodeSystem>(fixedUpdateGroup);
            world.AddSystemsGroup((int)SystemGroupOrder.FixedUpdateGroup, fixedUpdateGroup);

            var lateUpdateGroup = world.CreateSystemsGroup();            
            AddSystem<TransformsSyncSystem>(lateUpdateGroup);
            AddSystem<ViewDestroyEffectSystem>(lateUpdateGroup);
            world.AddSystemsGroup((int)SystemGroupOrder.LateUpdateGroup, lateUpdateGroup);

            var clearGroup = world.CreateSystemsGroup();
            AddSystem<PathsClearSystem>(clearGroup);
            AddSystem<TransformsClearSystem>(clearGroup);
            AddSystem<CollidersClearSystem>(clearGroup);
            AddSystem<EntityDisposeSystem>(clearGroup);
            AddSystem<UpdateTagsClearSystem>(clearGroup);
            world.AddSystemsGroup((int)SystemGroupOrder.ClearSystems, clearGroup);

            world.Commit();
        }
    }
}
