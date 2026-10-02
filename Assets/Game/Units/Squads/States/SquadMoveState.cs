using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class SquadMoveState : StateHandler
    {
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<SquadUpdatedTag> _squadUpdatedTags;
        private readonly SquadHandler _squadHandler;


        [Inject]
        public SquadMoveState(
            World world, 
            SquadHandler squadHandler, 
            INavigationMap navigationMap, 
            MoveTargetApplier moveTargetApplier)
        {
            _squadHandler = squadHandler;

            _moveTargets = world.GetStash<MoveTargetComponent>();
            _squadUpdatedTags = world.GetStash<SquadUpdatedTag>();
        }

        public override void Enter(Entity entity) 
        {
            //UnityEngine.Debug.Log("ordered to move");
        }

        public override void Exit(Entity entity) { }

        public override StateKey Update(Entity entity, float dt)
        {
            if (!_squadUpdatedTags.Has(entity))
            {
                var stoppedEntitiesFound = false;
                foreach (var (memberEntity, memberIndex) in _squadHandler.GetNextSquadMember(entity))
                {
                    if (_moveTargets.Has(memberEntity))
                        continue;

                    stoppedEntitiesFound = true;
                }
                if (!stoppedEntitiesFound) 
                    return StateKey.Move;
            }
               

            var targetComponent = _moveTargets.Get(entity);
            var allUnitsPositionMatch = _squadHandler.RecalculateMembersPositions(entity, targetComponent.WorldPos, targetComponent.TriangularPos);

            return allUnitsPositionMatch ? StateKey.Idle : StateKey.Move;
        }        
    }
}
