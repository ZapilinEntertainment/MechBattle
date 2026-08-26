using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechMovement;
using ZE.MechBattle.MechBuilding;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class MechFeatureInstaller : EcsFeatureModule<MechSystemsQueue>, ISessionAsyncResourceLoader
    {
        protected override MechSystemsQueue CreateQueue() => new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<MechCreateRequestsFactory>(Lifetime.Scoped);
            builder.Register<MechChassisFactory>(Lifetime.Scoped);
            builder.Register<MechMovementHandler>(Lifetime.Scoped);            
            builder.Register<MechInterpolator>(Lifetime.Scoped);
            builder.Register<MechHandler>(Lifetime.Scoped);

            builder.Register<IMechStepsMap, MechStepsMap>(Lifetime.Scoped);

            builder.Register<MechFactory>(Lifetime.Scoped);
            builder.Register<MechBuilder>(Lifetime.Transient);
            builder.Register<MechBitsBuilder>(Lifetime.Transient);
            builder.Register<MechWeaponsBuilder>(Lifetime.Transient);
            builder.Register<MechPartitionBuilder>(Lifetime.Transient);

            builder.Register<MechPartitionFactory>(Lifetime.Scoped);
            builder.Register<PartitionsListManager>(Lifetime.Scoped);

#if UNITY_EDITOR
            builder.Register<StepDrawer>(Lifetime.Scoped);
#endif
        }

        async Awaitable<IResourceBinder> ISessionAsyncResourceLoader.LoadSessionResourcesAsync(IObjectResolver resolver)
        {
            const string viewKey = DevelopConstants.DEFAULT_MECH_ID + "_chassis_data";
            var mechChassisData = await AssetsManager.LoadAssetDirectly<MechChassisData>(viewKey);

            const string configKey = DevelopConstants.DEFAULT_MECH_ID + "_config";
            var mechConfigData = await AssetsManager.LoadAssetDirectly<MechConfig>(configKey);

            var bindersList = new IResourceBinder[2]
            {
                new KeyedResourceBinding<MechChassisData, string>(mechChassisData, DevelopConstants.DEFAULT_MECH_ID),
                new KeyedResourceBinding<MechConfig, string>(mechConfigData, DevelopConstants.DEFAULT_MECH_ID)
            };
            return new AsyncResourcesScopeBinder(bindersList);
        }
    }
}
