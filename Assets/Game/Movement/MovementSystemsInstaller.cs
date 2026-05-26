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
            RegisterSystem<HexPathSearchSystem>();
            RegisterSystem<HexPortalPathCalculationSystem>();
            RegisterSystem<HexPortalPathAccountingSystem>();           
            
            RegisterSystem<TrianglePathDefineSystem>();
            RegisterSystem<FlowPathSearchSystem>();

            RegisterSystem<TrianglePathsAccountingSystem>();
            RegisterSystem<FlowMapsAccountingSystem>();
            RegisterSystem<TrianglePathCalculationSystem>();
            

            RegisterSystem<TrianglePathWaypointSetSystem>();
            RegisterSystem<WaypointsMovementSystem>();
            RegisterSystem<TrianglePathProgressionUpdateSystem>();
            RegisterSystem<HexPathProgressionUpdateSystem>();

            RegisterSystem<ChangeMovementTargetSystem>();
            RegisterSystem<HexPortalPathClearSystem>();
            RegisterSystem<TrianglePathClearSystem>();

            builder.Register<HexRaycastRequestsList>(Lifetime.Scoped);
            
            builder.Register<TrianglePathsLRUBuffer>(_ => new(), Lifetime.Scoped);

            builder.Register<HexPortalsList>(Lifetime.Scoped);
            builder.Register<PortalConnectionsList>(Lifetime.Scoped);
            builder.Register<HexPortalPathsLRUBuffer>(Lifetime.Scoped);
           
            builder.Register<FlowMapsCoordinator>(Lifetime.Scoped);
            builder.Register<FlowMapsFactory>(Lifetime.Scoped);
            builder.Register<PortalFlowMapsList>(Lifetime.Scoped);
            builder.Register<FlowMapAssignmentList>(Lifetime.Scoped);

            builder.Register<NavigationMapInitializer>(Lifetime.Transient);
        }

        // TODO: add triangle path systems
        public static void Install(SystemsResolver resolver)
        {
            resolver.AddSystem<TriangularPosUpdateSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<NoTargetPathsClearingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<HexPathDefineSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathSearchSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPortalPathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPortalPathAccountingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathDefineSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<FlowPathSearchSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathsAccountingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathWaypointSetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<WaypointsMovementSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<ChangeMovementTargetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPortalPathClearSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathClearSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddInitializer<NavigationMapInitializer>(SystemGroupOrder.Initialization);
        }

    }
}
