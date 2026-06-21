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

            RegisterSystem<HexRaycastUpdateSystem>();
            RegisterSystem<ActualEdgeExitDataCalculationSystem>();
            RegisterSystem<PortalEdgeExitsUpdateSystem>();
            RegisterSystem<PortalsActualizationSystem>();
            
            RegisterSystem<OutdatedExitsClearSystem>();
            RegisterSystem<OutdatedPortalsClearSystem>();
            RegisterSystem<PortalDistancesCalculationSystem>();

            RegisterSystem<TriangularPosUpdateSystem>();
            RegisterSystem<NoTargetPathsClearingSystem>();

            RegisterSystem<HexPathDefineSystem>();
            RegisterSystem<HexPathSearchSystem>();
            RegisterSystem<HexPortalPathCalculationSystem>();
            RegisterSystem<HexPortalPathAccountingSystem>();  
            RegisterSystem<HexPathReadyCheckSystem>();
            
            RegisterSystem<TrianglePathDefineSystem>();
            RegisterSystem<FlowPathSearchSystem>();
            RegisterSystem<FlowMapCalculationSystem>();
            RegisterSystem<TrianglePathSearchSystem>();
            RegisterSystem<TrianglePathCalculationSystem>();
            RegisterSystem<RegularTrianglePathReadyCheckSystem>();
            RegisterSystem<FlowTrianglePathReadyCheckSystem>();

            RegisterSystem<RegularTrianglePathsAccountingSystem>();
            RegisterSystem<FlowMapsAccountingSystem>();           

            RegisterSystem<RegularTrianglePathWaypointSetSystem>();
            RegisterSystem<FlowTrianglePathWaypointSetSystem>();

            RegisterSystem<WaypointsMovementSystem>();
            RegisterSystem<TrianglePathProgressionUpdateSystem>();
            RegisterSystem<HexPathProgressionUpdateSystem>();

            RegisterSystem<PortalsPathInvalidationSystem>();
            RegisterSystem<ChangeMovementTargetSystem>();
            RegisterSystem<HexPortalPathClearSystem>();
            RegisterSystem<TrianglePathClearSystem>();

            builder.Register<HexRaycastRequestsList>(Lifetime.Scoped);
            builder.Register<UpdateEdgeExitsRequestsList>(Lifetime.Scoped);            
            builder.Register<TrianglePathsLRUBuffer>(_ => new(), Lifetime.Scoped);            
            builder.Register<HexPortalPathsLRUBuffer>(Lifetime.Scoped);

            builder.Register<UpdatedPortalExitsList>(Lifetime.Scoped);
            builder.Register<PortalExitsUpdateDataPool>(Lifetime.Scoped);
            builder.Register<OutdatedExitsList>(Lifetime.Scoped);
            builder.Register<UpdatePortalRequestsList>(Lifetime.Scoped);
            builder.Register<OutdatedPortalsList>(Lifetime.Scoped);
            builder.Register<PortalDistancesCalculationRequests>(Lifetime.Scoped);

            builder.Register<IHexPortalsList, HexPortalsList>(Lifetime.Scoped).AsSelf();
            builder.Register<IPortalExitsList, PortalExitsList>(Lifetime.Scoped).AsSelf();
            builder.Register<PortalConnectionsList>(Lifetime.Scoped);
            builder.Register<IHexPortalsCoordinator, HexPortalsCoordinator>(Lifetime.Scoped).AsSelf();
            builder.Register<IPortalsLogic, HexPortalsLogic>(Lifetime.Scoped);
            builder.Register<IExitsLogic, HexExitsLogic>(Lifetime.Scoped);

            builder.Register<HexDataCoordinator>(Lifetime.Scoped);
            builder.Register<FlowMapsFactory>(Lifetime.Scoped);
            builder.Register<PortalFlowMapsList>(Lifetime.Scoped);
            builder.Register<FlowMapAssignmentList>(Lifetime.Scoped);

            builder.Register<NavigationMapInitializer>(Lifetime.Transient);
        }

        // TODO: add triangle path systems
        public static void Install(SystemsResolver resolver)
        {
            resolver.AddSystem<HexRaycastUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<ActualEdgeExitDataCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<PortalEdgeExitsUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<PortalsActualizationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<OutdatedExitsClearSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<OutdatedPortalsClearSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<PortalDistancesCalculationSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TriangularPosUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<NoTargetPathsClearingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<HexPathDefineSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathSearchSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPortalPathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPortalPathAccountingSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathReadyCheckSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<TrianglePathDefineSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<FlowPathSearchSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<FlowMapCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathSearchSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<RegularTrianglePathReadyCheckSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<FlowTrianglePathReadyCheckSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<RegularTrianglePathsAccountingSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<RegularTrianglePathWaypointSetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<FlowTrianglePathWaypointSetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<WaypointsMovementSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddSystem<PortalsPathInvalidationSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<ChangeMovementTargetSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<HexPortalPathClearSystem>(SystemGroupOrder.RegularUpdate);
            resolver.AddSystem<TrianglePathClearSystem>(SystemGroupOrder.RegularUpdate);

            resolver.AddInitializer<NavigationMapInitializer>(SystemGroupOrder.Initialization);
        }

    }
}
