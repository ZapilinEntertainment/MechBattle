using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public static class MovementSystemsInstaller
    {
        public static void RegisterSystems(IContainerBuilder builder)
        {
            void RegisterSystem<T>() where T : ISystem => builder.Register<T>(Lifetime.Scoped);

            RegisterSystem<TriangularPosUpdateSystem>();
            RegisterSystem<NoTargetPathsClearingSystem>();

            RegisterSystem<HexPathDefineSystem>();
            RegisterSystem<HexPathCalculationSystem>();
            RegisterSystem<HexPathAccountingSystem>();

            RegisterSystem<TrianglePathDefineSystem>();
            RegisterSystem<TrianglePathCalculationSystem>();
            RegisterSystem<TrianglePathsAccountingSystem>();

            RegisterSystem<TrianglePathWaypointSetSystem>();
            RegisterSystem<WaypointsMovementSystem>();
            RegisterSystem<TrianglePathProgressionUpdateSystem>();
            RegisterSystem<HexPathProgressionUpdateSystem>();

            RegisterSystem<ChangeMovementTargetSystem>();
            RegisterSystem<HexPathClearSystem>();
            RegisterSystem<TrianglePathClearSystem>();

            builder.Register<HexRaycastRequestsList>(Lifetime.Scoped);
            
            builder.Register<TrianglePathsLRUBuffer>(_ => new(), Lifetime.Scoped);
            builder.Register<HexPathsSearchHistory>(Lifetime.Scoped);
            builder.Register<RequestedHexPathsList>(Lifetime.Scoped);
            builder.Register<HexPathsLRUBuffer>(Lifetime.Scoped);

            builder.Register<PortalsList>(Lifetime.Scoped);
            builder.Register<PortalConnectionsList>(Lifetime.Scoped);

            builder.Register<NavigationMapInitializer>(Lifetime.Transient);
        }

        // TODO: add triangle path systems
        public static void Install(SystemsResolver resolver)
        {
            resolver.AddSystem<TriangularPosUpdateSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<NoTargetPathsClearingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<HexPathDefineSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathAccountingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathDefineSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathsAccountingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathWaypointSetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<WaypointsMovementSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<ChangeMovementTargetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathClearSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathClearSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddInitializer<NavigationMapInitializer>(SystemGroupOrder.Initialization);
        }

    }
}
