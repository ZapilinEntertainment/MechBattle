using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.MechMovement;
using ZE.MechBattle.MechBuilding;
using ZE.MechBattle.Mech.UI;
using VContainer.Unity;

namespace ZE.MechBattle
{
    [System.Serializable]
    public class MechFeatureInstaller : 
        EcsFeatureModule<MechSystemsQueue>, 
        ISessionAsyncResourceLoader,
        IAsyncWindowLoader,
        IEntityStateInstaller
    {
        protected override MechSystemsQueue CreateQueue() => new();

        private class MechBotStatesInstaller : FeatureStateInstallerBase
        {
            public MechBotStatesInstaller()
            {
                States = new IEntityStateInstaller[]
                {
                    new StateInstaller<MechBotChassisIdleState>(BehaviourKey.MechBotChassis, StateKey.Idle),
                    new StateInstaller<MechBotChassisAttackState>(BehaviourKey.MechBotChassis, StateKey.Attack),
                    new StateInstaller<MechBotChassisMoveState>(BehaviourKey.MechBotChassis, StateKey.Move),

                    new StateInstaller<MechBotIdleState>(BehaviourKey.MechBotCabin, StateKey.Idle),
                    new StateInstaller<MechBotMoveState>(BehaviourKey.MechBotCabin, StateKey.Move),
                    new StateInstaller<MechBotAttackState>(BehaviourKey.MechBotCabin, StateKey.Attack),
                };
            }
        }
        private MechBotStatesInstaller _statesInstaller = new();

        public override void SceneScopeInstall(IContainerBuilder builder)
        {
            base.SceneScopeInstall(builder);
            builder.Register<MechCreateRequestsFactory>(Lifetime.Singleton);
            builder.Register<MechChassisFactory>(Lifetime.Singleton);                      
            builder.Register<MechInterpolator>(Lifetime.Singleton);

            builder.Register<MechHandler>(Lifetime.Singleton);
            builder.Register<MechWeaponsHandler>(Lifetime.Singleton);
            builder.Register<MechMovementHandler>(Lifetime.Singleton);
            builder.Register<MechBotHandler>(Lifetime.Singleton);
            builder.Register<MechControlsHandler>(Lifetime.Singleton);

            builder.Register<IMechStepsMap, MechStepsMap>(Lifetime.Singleton);

            builder.Register<MechFactory>(Lifetime.Singleton);
            builder.Register<MechBuilder>(Lifetime.Transient);
            builder.Register<MechBitsBuilder>(Lifetime.Transient);
            builder.Register<MechWeaponsBuilder>(Lifetime.Transient);
            builder.Register<MechPartitionBuilder>(Lifetime.Transient);

            builder.Register<MechControlsWorker>(Lifetime.Transient);
            builder.Register<LaserEyesControlsWorker>(Lifetime.Transient);

            builder.Register<IMechUIElementsVisibilityController, UIMechInterfaceWorker>(Lifetime.Transient).AsSelf();
            builder.RegisterEntryPoint<MechUiInitializer>(Lifetime.Transient);

            builder.Register<RepairFeatureApplier>(Lifetime.Singleton);

            builder.Register<MechPartitionFactory>(Lifetime.Singleton);
            builder.Register<PartitionsListManager>(Lifetime.Singleton);

           

#if UNITY_EDITOR
            builder.Register<StepDrawer>(Lifetime.Singleton);
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

        IWindowBinder IAsyncWindowLoader.GetWindowBinder()
        {
            return new WindowBinder<UIMechInterfaceWindow>();
        }

        public void BindStates(IContainerBuilder builder) => _statesInstaller.BindStates(builder);
        public void AddStatesToDictionary(StateBehavioursDictionary dictionary) => _statesInstaller.AddStatesToDictionary(dictionary);
    }
}
