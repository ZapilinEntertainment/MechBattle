using System.Collections.Generic;
using VContainer;

namespace ZE.MechBattle.Ecs.States
{
    [System.Serializable]
    public class EntityStateFeatureModule : IFeatureModule, ISceneFeatureScopeInstaller, ISceneFeatureInitializer
    { 
        void ISceneFeatureScopeInstaller.SceneScopeInstall(IContainerBuilder builder)
        {
            builder.Register<StatesApplier>(Lifetime.Singleton);

            builder.Register<DefaultUnitIdleState>(Lifetime.Transient);
            builder.Register<DefaultUnitMoveState>(Lifetime.Transient);
            builder.Register<DefaultUnitAttackState>(Lifetime.Transient);

            builder.Register<StateBehavioursDictionary>(Lifetime.Singleton);

            builder.Register<StateUpdateSystem<RegularPriorityStateMachineTag>>(Lifetime.Transient);
            builder.Register<StateUpdateSystem<HighPriorityStateMachineTag>>(Lifetime.Transient);
        }

        void ISceneFeatureInitializer.OnSceneContainerBuilt(IObjectResolver resolver)
        {
            var systemsResolver = resolver.Resolve<MorpehSystemInstallHandler>();
            systemsResolver.AddSystem<StateUpdateSystem<HighPriorityStateMachineTag>>(SystemGroupOrder.RegularUpdate);
            systemsResolver.AddSystem<StateUpdateSystem<RegularPriorityStateMachineTag>>(SystemGroupOrder.RegularUpdate);
        }
    }
}
