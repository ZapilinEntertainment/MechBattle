using Scellecs.Morpeh;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public abstract class MechBotStateBase : StateHandler
    {
        protected abstract StateKey StateKey { get; }
        protected readonly MechHandler _mechHandler;
        protected readonly MoveTargetApplier _moveTargetApplier;
        protected readonly Stash<StateComponent> _stateComponents;
        

        public MechBotStateBase(MechHandler mechHandler, World world, MoveTargetApplier moveTargetApplier)
        {
            _mechHandler = mechHandler;
            _moveTargetApplier = moveTargetApplier;
            _stateComponents = world.GetStash<StateComponent>();
        }

        protected void SyncStateWithChassis(Entity mechEntity, StateKey stateKey) => _stateComponents.Get(_mechHandler.GetChassisEntity(mechEntity)).NextState = stateKey;


        public override void Enter(Entity entity) => SyncStateWithChassis(entity, StateKey);
        public override void Exit(Entity entity) => SyncStateWithChassis(entity, StateKey);

        protected void SetMoveTarget(Entity mechEntity, float3 worldPos) => _moveTargetApplier.SetMoveTarget(_mechHandler.GetChassisEntity(mechEntity), worldPos);
    }
}
