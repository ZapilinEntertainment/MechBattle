using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using UnityEngine;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract class FiringLineCheckSystemBase<T> : PausableSystem
        where T : unmanaged, IFirelineCalculationTag
    {
        protected bool _isJobActive = false;
        protected JobHandle _activeJobHandle;
        protected NativeList<RaycastCommand> _commandsList;
        protected NativeList<RaycastHit> _resultsList;

        protected Stash<AttackTargetComponent> _attackTargets;
        protected Stash<FireLineClearTag> _fireLineClearTag;
        protected Stash<WeaponShotPoint> _weaponShotPoints;
        protected Stash<WeaponRangeComponent> _weaponRanges;

        protected Filter _filter;

        protected readonly List<EntityFirelineCheckProtocol> _activeJobEntities = new();
        protected readonly CollidersTable _collidersTable;
        protected readonly AffinityHandler _affinityHandler;
        protected readonly QueryParameters _queryParameters = new()
        {
            hitBackfaces = false,
            hitMultipleFaces = false,
            hitTriggers = QueryTriggerInteraction.Ignore,
            layerMask = LayerConstants.FirelineCastMask
        };

        public FiringLineCheckSystemBase(CollidersTable collidersTable, AffinityHandler affinityHandler, SceneFlagsManager flags) : base(flags)
        {
            _collidersTable = collidersTable;
            _affinityHandler = affinityHandler;
        }

        public override void OnAwake()
        {
            _filter = GetFilterBuilder().Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _fireLineClearTag = World.GetStash<FireLineClearTag>();
            _weaponShotPoints = World.GetStash<WeaponShotPoint>();
            _weaponRanges = World.GetStash<WeaponRangeComponent>();

            _commandsList = new NativeList<RaycastCommand>(Allocator.Persistent);
            _resultsList = new NativeList<RaycastHit>(Allocator.Persistent);
        }

        public override void OnUpdate(float deltaTime)
        {
            if (_isJobActive & _activeJobHandle.IsCompleted)
            {
                ApplyPreviousJob();
                OnJobApplied();                
            }

            if (IsPaused || _filter.IsEmpty() || !_activeJobHandle.IsCompleted)
                return;

            TryLaunchNextJob();
        }

        protected override void InternalDispose()
        {
            _activeJobHandle.Complete();
            _commandsList.Dispose();
            _resultsList.Dispose();
        }

        protected void TryLaunchNextJob()
        {
            foreach (var entity in _filter)
            {
                var targetEntity = _attackTargets.Get(entity).Entity;
                if (World.IsDisposed(targetEntity))
                    continue;

                _activeJobEntities.Add(new(entity, targetEntity));
            }

            if (_activeJobEntities.Count != 0)
                PrepareAndLaunchJob();
        }

        protected virtual FilterBuilder GetFilterBuilder() =>
            World.Filter
              .With<AttackTargetComponent>()
              .With<T>()
              .With<AttackRangeReachedTag>();

        protected virtual void OnJobApplied()
        {
            _isJobActive = false;
            _activeJobEntities.Clear();
            _commandsList.Clear();
            _resultsList.Clear();
        }

        // returns true if fireline is not obstructed completely or obstructed by enemy
        protected bool IsFirelineClear(int resultsIndex, Entity weaponEntity, Entity targetEntity)
        {
            var result = _resultsList[resultsIndex];
            if (result.colliderInstanceID != 0)
            {
                // if raycast hit actual target or at least hit enemy:
                return
                    _collidersTable.TryGetColliderOwner(result.colliderInstanceID, out var colliderOwnerEntity)
                    && (
                        colliderOwnerEntity == targetEntity
                        || _affinityHandler.AreEntitiesHostile(_affinityHandler.GetEntityOwner(weaponEntity), targetEntity)
                    );
            }

            return true;
        }

        protected abstract void ApplyPreviousJob();
        protected abstract void PrepareAndLaunchJob();
       
    }
}