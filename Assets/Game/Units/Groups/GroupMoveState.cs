using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;

namespace ZE.MechBattle.Ecs.States
{
    public class GroupMoveState : StateHandler
    {
        private readonly World _world;
        private readonly Stash<MoveTargetComponent> _moveTargets;
        private readonly Stash<PositionComponent> _positions;

        [Inject]
        public GroupMoveState(World world)
        {
            _world = world;
            _moveTargets = _world.GetStash<MoveTargetComponent>();
            _positions = _world.GetStash<PositionComponent>();
        }

        public override void Enter(Entity entity)
        {
        }

        public override void Exit(Entity entity)
        {
        }

        public override StateKey Update(Entity entity, float dt)
        {
            var position = _positions.Get(entity).Value;
            var targetPos = _moveTargets.Get(entity).Value;
            if (math.lengthsq(targetPos - position) > math.EPSILON)
            {
                var job = new PositionGroupEntitiesJob();
                _world.JobHandle = job.Schedule(_world.JobHandle);
                return StateKey.Move;
            }
            _moveTargets.Remove(entity);
            return StateKey.Idle;
        }

        [BurstCompile]
        private struct PositionGroupEntitiesJob : IJob
        {
            [ReadOnly] public float3 ZeroPos;
            [ReadOnly] public int RowsCount;
            [ReadOnly] public int ColumnsCount;
            [ReadOnly] public NativeArray<Entity> Entities;
            [ReadOnly] public NativeStash<UnitGroupComponent> GroupComponents;
            public NativeStash<MoveTargetComponent> MoveTargets;

            public void Execute()
            {
                foreach (var entity in Entities)
                {

                }
            }
        }
    }
}
