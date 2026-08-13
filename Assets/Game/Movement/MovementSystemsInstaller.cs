using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    [System.Serializable]
    public class MovementSystemsInstaller : EcsFeatureModule<MovementSystemsInstallQueue>
    {
        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);

            builder.Register<TransformAspectHandler>(Lifetime.Scoped);

            builder.Register<HexRaycastRequestsList>(Lifetime.Scoped);
            builder.Register<UpdateEdgeExitsRequestsList>(Lifetime.Scoped);
            builder.Register<TrianglePathsLRUBuffer>(_ => new(), Lifetime.Scoped);
            builder.Register<IPortalPaths, HexPortalPathsLRUBuffer>(Lifetime.Scoped).AsSelf();

            builder.Register<UpdatedPortalExitsList>(Lifetime.Scoped);
            builder.Register<PortalExitsUpdateDataPool>(Lifetime.Scoped);
            builder.Register<OutdatedExitsList>(Lifetime.Scoped);
            builder.Register<UpdatePortalRequestsList>(Lifetime.Scoped);
            builder.Register<OutdatedPortalsList>(Lifetime.Scoped);
            builder.Register<PortalDistancesCalculationRequests>(Lifetime.Scoped);

            builder.Register<IHexPortalsList, HexPortalsList>(Lifetime.Scoped).AsSelf();
            builder.Register<IPortalExitsList, PortalExitsList>(Lifetime.Scoped).AsSelf();
            builder.Register<IPortalConnectionsList, PortalConnectionsList>(Lifetime.Scoped).AsSelf();
            builder.Register<IHexPortalsCoordinator, HexPortalsCoordinator>(Lifetime.Scoped).AsSelf();
            builder.Register<IPortalsLogic, HexPortalsLogic>(Lifetime.Scoped);
            builder.Register<IExitsLogic, HexExitsLogic>(Lifetime.Scoped);

            builder.Register<HexDataCoordinator>(Lifetime.Scoped);
            builder.Register<FlowMapsFactory>(Lifetime.Scoped);
            builder.Register<IFlowMapsList, PortalFlowMapsList>(Lifetime.Scoped).AsSelf();
            builder.Register<FlowMapAssignmentList>(Lifetime.Scoped);

            builder.Register<IMovementCellsMap, MovementCellsMap>(Lifetime.Scoped).AsSelf();

            builder.RegisterEntryPoint<NavigationMapInitializer>();
        }

        protected override MovementSystemsInstallQueue CreateQueue() => new();
    }
}
