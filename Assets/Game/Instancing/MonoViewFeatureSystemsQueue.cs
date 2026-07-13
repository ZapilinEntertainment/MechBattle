using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class MonoViewFeatureSystemsQueue : FeatureSystemsInstallQueue
    {
        protected override void Configure(ISystemsOperator installer)
        {
            installer.AddSystem<ViewRequestsHandleSystem>(SystemGroupOrder.ViewsLoading);
            installer.AddSystem<UpdateChildViewLinkSystem>(SystemGroupOrder.ViewsLoading);
        }
    }
}
