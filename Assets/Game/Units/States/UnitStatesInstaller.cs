using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class UnitStatesInstaller : FeatureStateInstallerBase
    {
        public UnitStatesInstaller()
        {
            States = new IEntityStateInstaller[]
            {
                new StateInstaller<DefaultUnitIdleState>(BehaviourKey.Tank, StateKey.Idle),
                new StateInstaller<DefaultUnitMoveState>(BehaviourKey.Tank, StateKey.Move),
                new StateInstaller<DefaultUnitAttackState>(BehaviourKey.Tank, StateKey.Attack),
            };
        }
        
    }
}
