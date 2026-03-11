using System.Collections.Generic;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public static class MorpehInstaller
    {

        public static void SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<World>(_ => CreateWorld(), Lifetime.Scoped);

            builder.Register<ProjectileRequestsFactory>(Lifetime.Scoped);
            builder.Register<ProjectileBuilder>(Lifetime.Scoped);
            builder.Register<ProjectileViewBuilder>(Lifetime.Scoped);
            builder.Register<ExplosionRequestsBuilder>(Lifetime.Scoped);
            builder.Register<DamageRequestsBuilder>(Lifetime.Scoped);
            builder.Register<VfxRequestsBuilder>(Lifetime.Scoped);
            builder.Register<EntityFactory>(Lifetime.Scoped);

            void RegisterSystem<T>() where T : class, ISystem => builder.Register<T>(Lifetime.Transient);
            void RegisterInitializer<T>() where T : class, IInitializer => builder.Register<T>(Lifetime.Transient);

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

            StatesInstaller.RegisterStates(builder);
            MovementSystemsInstaller.RegisterSystems(builder);
        }
        private static World CreateWorld()
        {
            var world = World.Create();
            // NOTE: NECESSARY!
            world.UpdateByUnity = true;
            //UnityEngine.Debug.Log($"registered: {world.GetHashCode()}");
            return world;
        }

        public static void OnSceneDependenciesResolved(IObjectResolver resolver)
        {            
            //UnityEngine.Debug.Log($"resolved: {world.GetHashCode()}");
            var systemResolver = new SystemsResolver(resolver);

            void AddSystem<T>(SystemGroupOrder order) where T : class, ISystem => systemResolver.AddSystem<T>(order);
            void AddInitializer<T>(SystemGroupOrder order) where T : class, IInitializer => systemResolver.AddInitializer<T>(order);

            AddInitializer<SceneInitializer>(SystemGroupOrder.Initialization);

            AddInitializer<DamageablesInitializer>(SystemGroupOrder.Default);
            AddInitializer<SceneUnitsInitializer>(SystemGroupOrder.Default);
            AddSystem<ViewRequestsHandleSystem>(SystemGroupOrder.Default);
            AddSystem<StateUpdateSystem>(SystemGroupOrder.Default);
            AddSystem<ProjectileCreateSystem>(SystemGroupOrder.Default);     
            AddSystem<DamageCalculationSystem>(SystemGroupOrder.Default);
            AddSystem<DamageApplySystem>(SystemGroupOrder.Default);
            AddSystem<VfxCreateSystem>(SystemGroupOrder.Default);
            AddSystem<RestorationSystem>(SystemGroupOrder.Default);

            AddSystem<ProjectileMoveSystem>(SystemGroupOrder.RegularUpdate);
            AddSystem<ProjectilesExplodeSystem>(SystemGroupOrder.RegularUpdate);

            MovementSystemsInstaller.Install(systemResolver);
       
            AddSystem<TransformsSyncSystem>(SystemGroupOrder.PostUpdate);
            AddSystem<ViewDestroyEffectSystem>(SystemGroupOrder.PostUpdate);

            AddSystem<TransformsClearSystem>(SystemGroupOrder.Final);
            AddSystem<CollidersClearSystem>(SystemGroupOrder.Final);
            AddSystem<EntityDisposeSystem>(SystemGroupOrder.Final);
            AddSystem<UpdateTagsClearSystem>(SystemGroupOrder.Final);

            systemResolver.ApplySystems();
        }
    }
}
