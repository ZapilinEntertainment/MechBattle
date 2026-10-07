using UnityEngine;
using ZE.MechBattle.Movement.CollisionAvoidance;

namespace ZE.MechBattle.Ecs
{
    public class MovementSystemsInstallQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<HexRaycastUpdateSystem>(SystemGroupOrder.BeforePathfinding);
            installer.AddSystem<ActualEdgeExitDataCalculationSystem>(SystemGroupOrder.BeforePathfinding);
            installer.AddSystem<PortalEdgeExitsUpdateSystem>(SystemGroupOrder.BeforePathfinding);
            installer.AddSystem<PortalsActualizationSystem>(SystemGroupOrder.BeforePathfinding);

            installer.AddSystem<OutdatedExitsClearSystem>(SystemGroupOrder.BeforePathfinding);
            installer.AddSystem<OutdatedPortalsClearSystem>(SystemGroupOrder.BeforePathfinding);
            installer.AddSystem<PortalDistancesCalculationSystem>(SystemGroupOrder.BeforePathfinding);

            installer.AddSystemWithInterface<TriangularPosUpdateSystem, ITrianglePositionCalculator>(TriangularPosUpdateSystem.GroupOrder);
            installer.AddSystem<NoTargetPathsClearingSystem>(SystemGroupOrder.BeforePathfinding);

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
            installer.AddSystem<FlowPathDirectionSetSystem>(SystemGroupOrder.Pathfinding);
            installer.AddSystem<FlowPathCorrectionApplySystem>(SystemGroupOrder.Pathfinding);
            // --- requires job completion---
            installer.AddSystem<WaypointsMovementSystem>(SystemGroupOrder.UnitsNextPositionCalculation2);            

            installer.AddSystem<NextPositionApplySystem>(SystemGroupOrder.UnitsMovement);
            installer.AddSystem<NextPositionsClearSystem>(SystemGroupOrder.UnitsMovement2);
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
