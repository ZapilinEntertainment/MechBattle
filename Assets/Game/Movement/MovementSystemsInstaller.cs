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

            RegisterSystem<HexPathUpdateSystem>();
            RegisterSystem<HexPathCalculationSystem>();

            RegisterSystem<TrianglePathAssignSystem>();
            RegisterSystem<TrianglePathCalculationSystem>();
            RegisterSystem<TrianglePathsAccountingSystem>();

            RegisterSystem<TrianglePathWaypointSetSystem>();
            RegisterSystem<WaypointsMovementSystem>();
            RegisterSystem<TrianglePathProgressionUpdateSystem>();
            RegisterSystem<HexPathProgressionUpdateSystem>();

            RegisterSystem<HexPathClearSystem>();
            RegisterSystem<TrianglePathClearSystem>();

            builder.Register<NavigationHexPathsList>(_ => new(), Lifetime.Scoped);
            builder.Register<NavigationTrianglePathsBuffer>(_ => new(), Lifetime.Scoped);

            builder.Register<NavigationMapInitializer>(Lifetime.Transient);

            builder.Register<HexPathSearcher>(Lifetime.Scoped);

        }

        // TODO: add triangle path systems
        public static void Install(SystemsResolver resolver)
        {
            resolver.AddSystem<TriangularPosUpdateSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<NoTargetPathsClearingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<HexPathUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathCalculationSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathAssignSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathsAccountingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathWaypointSetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<WaypointsMovementSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<HexPathClearSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathClearSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddInitializer<NavigationMapInitializer>(SystemGroupOrder.Initialization);
        }

    }
}
