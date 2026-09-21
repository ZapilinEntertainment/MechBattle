using UnityEngine;

namespace ZE.MechBattle.Ecs
{
    public class MovementSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<HexRaycastUpdateSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<ActualEdgeExitDataCalculationSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<PortalEdgeExitsUpdateSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<PortalsActualizationSystem>(SystemGroupOrder.Pathfinding);

            installer.AddSystem<OutdatedExitsClearSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<OutdatedPortalsClearSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<PortalDistancesCalculationSystem>(SystemGroupOrder.Pathfinding);

            installer.AddSystem<TriangularPosUpdateSystem>(TriangularPosUpdateSystem.GroupOrder);
            installer.AddSystem<NoTargetPathsClearingSystem>(SystemGroupOrder.Pathfinding);

            installer.AddSystem<HexPathDefineSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<HexPathSearchSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<HexPortalPathCalculationSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<HexPortalPathAccountingSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<HexPathReadyCheckSystem>(SystemGroupOrder.Pathfinding);

            installer.AddSystem<TrianglePathDefineSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<FlowPathSearchSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<FlowMapCalculationSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<TrianglePathSearchSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<TrianglePathCalculationSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<RegularTrianglePathReadyCheckSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<FlowTrianglePathReadyCheckSystem>(SystemGroupOrder.Pathfinding);

            installer.AddSystem<RegularTrianglePathsAccountingSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<FlowMapsAccountingSystem>(SystemGroupOrder.Pathfinding);

            installer.AddSystem<RegularTrianglePathWaypointSetSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<FlowTrianglePathWaypointSetSystem>(SystemGroupOrder.Pathfinding);

            installer.AddSystem<WaypointsMovementSystem>(SystemGroupOrder.UnitsNextPositionCalculation);            

            installer.AddSystem<NextPositionApplySystem>(SystemGroupOrder.UnitsMovement);
            installer.AddSystem<WaypointsCheckSystem>(SystemGroupOrder.PostMovement);

            installer.AddSystem<TrianglePathProgressionUpdateSystem>(SystemGroupOrder.PostMovement);
            installer.AddSystem<HexPathProgressionUpdateSystem>(SystemGroupOrder.PostMovement);

            installer.AddSystem<PortalsPathInvalidationSystem>(SystemGroupOrder.PostMovement);
            installer.AddSystem<ChangeMovementTargetSystem>(SystemGroupOrder.PostMovement);
            installer.AddSystem<HexPortalPathClearSystem>(SystemGroupOrder.PostMovement);
            installer.AddSystem<TrianglePathClearSystem>(SystemGroupOrder.PostMovement);

            CollisionAvoidanceSubfeatureInstaller.InstallSystems(installer);
        }
    }
}
