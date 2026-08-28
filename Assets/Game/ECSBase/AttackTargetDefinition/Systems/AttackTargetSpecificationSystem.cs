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
        public World World { get; set;}
        private Filter _filter;
        private Stash<AttackTargetComponent> _targets;
        private Stash<CompositeTargetComponent> _compositeTargets;
        private Stash<CompositeTargetSpecifiedTag> _specifiedTags;

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly PartitionsListManager _partitionsManager;
        private readonly Dictionary<Entity, IReadOnlyCollection<Entity>> _targetParts = new();

        [Inject]
        public AttackTargetSpecificationSystem(PartitionsListManager partitionsListManager, TransformAspectHandler transformAspectHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _partitionsManager = partitionsListManager;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<AttackTargetComponent>()
                .Without<CompositeTargetSpecifiedTag>()
                .Build();

            _targets = World.GetStash<AttackTargetComponent>();
            _compositeTargets = World.GetStash<CompositeTargetComponent>();
            _specifiedTags = World.GetStash<CompositeTargetSpecifiedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var attackerEntity in _filter)
            {
                ref var targetComponent = ref _targets.Get(attackerEntity);
                var targetEntity = targetComponent.Entity;

                var compositeTargetComponent = _compositeTargets.Get(targetEntity, out var isCompositeTarget);
                if (isCompositeTarget)
                {
                    // only one mode realized atm;
                    //var mode = compositeTargetComponent.Mode;

                    if (!_targetParts.TryGetValue(targetEntity, out var list))
                    {
                        list = _partitionsManager.GetPartitionsList(targetEntity).Entities;
                        _targetParts.Add(targetEntity, list);
                    }

                    targetComponent.Entity = SelectClosestPart(attackerEntity, list);
                }
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