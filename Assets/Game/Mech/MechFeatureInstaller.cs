using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle
{
    public class MechFeatureInstaller : EcsFeatureInstaller<MechSystemsQueue>, ISessionAsyncResourceLoader
    {
        protected override MechSystemsQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<MechFactory>(Lifetime.Scoped);
            builder.Register<MechCreateRequestsFactory>(Lifetime.Scoped);
            builder.Register<MechChassisFactory>(Lifetime.Scoped);
            builder.Register<MechMovementHandler>(Lifetime.Scoped);
        }

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            const string viewKey = DevelopConstants.DEFAULT_MECH_VIEW_ID + "_chassis_data";
            var mechChassisData = await AssetsManager.LoadAssetDirectly<MechChassisData>(viewKey);
            return new KeyedResourceBinding<MechChassisData,string>(mechChassisData, DevelopConstants.DEFAULT_MECH_VIEW_ID);
        }
    }
}
