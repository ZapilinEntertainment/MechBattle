using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Ecs.States;

namespace ZE.MechBattle
{
    public class MechBotIdleState : MechBotStateBase
    {
        protected override StateKey StateKey => StateKey.Idle;

        private readonly MechWeaponsHandler _mechWeaponsHandler;
        private readonly AffinityHandler _affinityHandler;
        private readonly TransformAspectHandler _transformAspectHandler;

        private readonly Filter _targetsFilter;        
        private readonly Stash<AttackRangeComponent> _attackRanges;
        private readonly Stash<AttackTargetComponent> _attackTarget;

        [Inject]
        public MechBotIdleState(
            World world, 
            MechWeaponsHandler mechWeaponsHandler, 
            AffinityHandler affinityHandler,
            TransformAspectHandler transformAspectHandler,
            MechHandler mechHandler,
            MoveTargetApplier moveTargetApplier) 
            : base(mechHandler, world, moveTargetApplier)
        {
            _mechWeaponsHandler = mechWeaponsHandler;
            _affinityHandler = affinityHandler;
            _transformAspectHandler = transformAspectHandler;

            _targetsFilter = world.Filter
                .With<PrimaryTargetingObjectTag>()
                .With<PlayerAffiliationComponent>()
                .With<TriangularPosComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _attackRanges = world.GetStash<AttackRangeComponent>();
            _attackTarget = world.GetStash<AttackTargetComponent>();
        }


        public override StateKey Update(Entity entity, float dt)
        {
            var attackRangeComponent = _attackRanges.Get(entity, out var attackRangeCalculated);
            if (!attackRangeCalculated)
            {
                var range = _mechWeaponsHandler.CalculateMechAttackRange(entity);
                _attackRanges.Set(entity, new(range));
                UnityEngine.Debug.Log($"{entity.Id} : {range}");
                return StateKey.Idle;
            }

            if (!_affinityHandler.TryGetPlayerOwner(entity, out var mechPlayerKey))
                return StateKey.Idle;

            var mechPosition = _transformAspectHandler.GetPosition(entity);
           


            Entity closestEnemy = default;
            var targetFound = false;
            var bestPoints = float.MinValue;

            foreach (var targetEntity in _targetsFilter)
            {
                if (targetEntity == entity 
                    || !_affinityHandler.IsEntityHostileToPlayer(targetEntity, mechPlayerKey))
                    continue;

                var targetDir = _transformAspectHandler.GetPosition(targetEntity) - mechPosition;
                var distance = math.length(targetDir);
                var dot = math.dot(targetDir / distance, math.forward());

                var points = math.clamp(distance / attackRangeComponent.Value, 0f, 1f) + dot * dot;
                //UnityEngine.Debug.Log($"{targetEntity.Id}: {distance / attackRangeComponent.Value} : {dot} : {points}");
                if (points > bestPoints)
                {
                    targetFound = true;
                    bestPoints = points;
                    closestEnemy = targetEntity;
                }
            }

            if (targetFound)
            {
                UnityEngine.Debug.Log($"target defined: {closestEnemy.Id}");
                _attackTarget.Set(entity, new() { Entity = closestEnemy });
                return StateKey.Attack;
            }
            else
            {
                return StateKey.Idle;
            }
        }


    }
}
