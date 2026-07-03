using UnityEngine;

namespace ZE.MechBattle.Ecs
{
    public class MovementSystemsConfigurator : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<HexRaycastUpdateSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<ActualEdgeExitDataCalculationSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<PortalEdgeExitsUpdateSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<PortalsActualizationSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<OutdatedExitsClearSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<OutdatedPortalsClearSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<PortalDistancesCalculationSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<TriangularPosUpdateSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<NoTargetPathsClearingSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<HexPathDefineSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<HexPathSearchSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<HexPortalPathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<HexPortalPathAccountingSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<HexPathReadyCheckSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<TrianglePathDefineSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<FlowPathSearchSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<FlowMapCalculationSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<TrianglePathSearchSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<TrianglePathCalculationSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<RegularTrianglePathReadyCheckSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<FlowTrianglePathReadyCheckSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<RegularTrianglePathsAccountingSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<FlowMapsAccountingSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<RegularTrianglePathWaypointSetSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<FlowTrianglePathWaypointSetSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<MovementVectorsMapUpdateSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<MovementCollisionAvoidanceSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<WaypointsMovementSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<NextPositionApplySystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<WaypointsCheckSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<TrianglePathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<HexPathProgressionUpdateSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddSystem<PortalsPathInvalidationSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<ChangeMovementTargetSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<HexPortalPathClearSystem>(SystemGroupOrder.RegularUpdate);
            installer.AddSystem<TrianglePathClearSystem>(SystemGroupOrder.RegularUpdate);

            installer.AddInitializer<NavigationMapInitializer>(SystemGroupOrder.Default);
        }
    }
}
