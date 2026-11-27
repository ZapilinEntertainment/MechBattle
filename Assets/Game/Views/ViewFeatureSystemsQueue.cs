using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class ViewFeatureSystemsQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<ViewRequestsHandleSystem>(SystemGroupOrder.ViewsLoading);
            installer.AddSystem<UpdateSubViewAwaitingEntitiesSystem>(SystemGroupOrder.ViewsLoading);
            installer.AddSystem<ViewPartAssignSystem>(SystemGroupOrder.ViewsLoading);

            installer.AddSystem<ViewPartsDisconnectionSystem>(SystemGroupOrder.DisposedObjectsOperations);
            installer.AddSystem<ViewsDisposeSystem>(SystemGroupOrder.DisposedObjectsOperations);

            installer.AddSystem<VisibilityLimitCheckSystem>(SystemGroupOrder.Default);
            installer.AddSystemWithJobScheduling<ViewVisibilityHexSectorCheckSystem>(SystemGroupOrder.Default);
            installer.AddSystem<VirtualViewsRenderSystem>(SystemGroupOrder.Default+1);

            installer.AddSystem<FinalViewApplicationSystem>(SystemGroupOrder.AfterDispose);

            // ILateSystem-s:
            installer.AddSystem<HexSectorsVisibilityRecalculationSystem>(SystemGroupOrder.EarlyUpdate);
            installer.AddSystemWithInterface<HexSectorsVisibilityCheckSystem, IVisibleHexList>(SystemGroupOrder.EarlyUpdate + 1);
        }
    }
}
