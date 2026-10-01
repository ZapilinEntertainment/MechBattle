using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class AttackTargetSpecificationSystem : ISystem 
    {
        // select an exact target entity for complex entity (ex.: mech hand or mech leg instead of mech root entity)
        // composite target marks itself with CompositeTargetComponent

        public World World { get; set;}

        private Filter _filter;
        private Filter _correctionFilter;
        private Stash<AttackTargetComponent> _targets;
        private Stash<CompositeTargetComponent> _compositeTargets;
        private Stash<CompositeTargetSpecifiedTag> _specifiedTags;

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly PartitionsListManager _partitionsManager;
        private readonly SquadHandler _squadHandler;
        private readonly Dictionary<Entity, IReadOnlyCollection<Entity>> _targetParts = new();

        [Inject]
        public AttackTargetSpecificationSystem(PartitionsListManager partitionsListManager, TransformAspectHandler transformAspectHandler, SquadHandler squadHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _partitionsManager = partitionsListManager;
            _squadHandler = squadHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<AttackTargetComponent>()
                .Without<CompositeTargetSpecifiedTag>()
                .Without<UseUnspecifiedTargetsTag>()
                .Build();

            _correctionFilter = World.Filter
                .With<AttackTargetComponent>()
                .With<CompositeTargetSpecifiedTag>()
                .With<SquadMemberComponent>()
                .Build();

            _targets = World.GetStash<AttackTargetComponent>();
            _compositeTargets = World.GetStash<CompositeTargetComponent>();
            _specifiedTags = World.GetStash<CompositeTargetSpecifiedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            // this is a kludge. Originals system should handle this situation themselves, hovewer it doesn't work
            // todo: investigate
            foreach (var entity in _correctionFilter)
            {
                var target = _targets.Get(entity).Entity;
                if (_compositeTargets.Has(target))
                {
                    _specifiedTags.Remove(entity);
                }
                    
            }

            foreach (var attackerEntity in _filter)
            {
                ref var targetComponent = ref _targets.Get(attackerEntity);
                var targetEntity = targetComponent.Entity;

                var compositeTargetComponent = _compositeTargets.Get(targetEntity, out var isCompositeTarget);
                var closestTargetFound = false;
                if (isCompositeTarget)
                {
                    switch (compositeTargetComponent.Mode)
                    {
                        case CompositeTargetMode.Partitions:
                            {
                                if (!_targetParts.TryGetValue(targetEntity, out var list))
                                {
                                    list = _partitionsManager.GetPartitionsList(targetEntity).Entities;
                                    _targetParts.Add(targetEntity, list);
                                }

                                targetComponent.Entity = SelectClosestPart(attackerEntity, list);
                                closestTargetFound = true;
                                break;
                            }
                        case CompositeTargetMode.Squad:
                            {
                                var closestDistance = float.MaxValue;
                                Entity closestTarget = default;
                                var attackerPos = _transformAspectHandler.GetPosition(attackerEntity);

                                foreach (var (entity, index) in _squadHandler.GetNextSquadMember(targetEntity))
                                {
                                    var pos = _transformAspectHandler.GetPosition(entity);
                                    var distanceSq = math.distancesq(pos, attackerPos);
                                    if (distanceSq < closestDistance)
                                    {
                                        closestDistance = distanceSq;
                                        closestTarget = entity;
                                    }
                                }
                                if (!World.IsDisposed(closestTarget))
                                {
                                    closestTargetFound = true;
                                    targetComponent.Entity = closestTarget;
                                }
                                break;
                            }
                    }
                }
                else
                {
                    closestTargetFound = true;
                }

                if (closestTargetFound)
                    _specifiedTags.Add(attackerEntity);                
            }

          
            _targetParts.Clear();
        }

        public void Dispose() { }

        private Entity SelectClosestPart(Entity attacker, IReadOnlyCollection<Entity> targetParts)
        {
            var attackerPos = _transformAspectHandler.GetPosition(attacker);
            var minDist = float.MaxValue;
            Entity newTarget = default;

            foreach (var targetPart in targetParts)
            {
                var pos = _transformAspectHandler.GetPosition(targetPart);
                var dist = math.distancesq(attackerPos, pos);
                if (dist < minDist)
                {
                    minDist = dist;
                    newTarget = targetPart;
                }
            }

            return newTarget;
        }
    }
}