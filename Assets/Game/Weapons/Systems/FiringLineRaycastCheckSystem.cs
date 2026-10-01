using System.Collections.Generic;
using Unity.Jobs;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using Unity.Collections;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FiringLineRaycastCheckSystem : ISystem 
    {
        private readonly struct EntityCastData
        {
            public readonly Entity WeaponEntity;
            public readonly Entity TargetEntity;

            public EntityCastData(Entity weaponEntity, Entity targetEntity)
            {
                WeaponEntity = weaponEntity;
                TargetEntity = targetEntity;
            }
        }

        public World World { get; set;}
        private Filter _filter;
        private bool _isJobActive = false;
        private JobHandle _activeJobHandle;
        private NativeList<RaycastCommand> _commandsList;
        private NativeList<RaycastHit> _resultsList;

        private Stash<AttackTargetComponent> _attackTargets;
        private Stash<FireLineClearTag> _fireLineClearTag;
        private Stash<WeaponShotPoint> _weaponShotPoints;
        private Stash<WeaponRangeComponent> _weaponRanges;

        private readonly AffinityHandler _affinityHandler;
        private readonly CollidersTable _collidersTable;
        private readonly List<EntityCastData> _activeJobEntities = new();
        private readonly QueryParameters _queryParameters = new()
        {
            hitBackfaces = false,
            hitMultipleFaces = false,
            hitTriggers = QueryTriggerInteraction.Ignore,
            layerMask = LayerConstants.FirelineCastMask
        };

        [Inject]
        public FiringLineRaycastCheckSystem(CollidersTable collidersTable, AffinityHandler affinityHandler)
        {
            _collidersTable = collidersTable;
            _affinityHandler = affinityHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<AttackTargetComponent>()                
                .With<CalculateFireLineByRaycastTag>()
                .With<AttackRangeReachedTag>()
                .Build();

            _attackTargets = World.GetStash<AttackTargetComponent>();
            _fireLineClearTag = World.GetStash<FireLineClearTag>();
            _weaponShotPoints = World.GetStash<WeaponShotPoint>();
            _weaponRanges = World.GetStash<WeaponRangeComponent>();

            _commandsList = new NativeList<RaycastCommand>(Allocator.Persistent);
            _resultsList = new NativeList<RaycastHit>(Allocator.Persistent);
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_isJobActive)
            {
                ApplyPreviousJob();
                _isJobActive = false;
            }

            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                var targetEntity = _attackTargets.Get(entity).Entity;
                _activeJobEntities.Add(new(entity, targetEntity));
            }

           if (_activeJobEntities.Count != 0)
                PrepareAndLaunchJob();

        }

        public void Dispose() 
        { 
            #if UNITY_EDITOR
            try 
            {
                FinalDispose();
            }
            catch
            {
                // editor-related problems
            }
#else
            FinalDispose();
#endif
        }

        private void FinalDispose()
        {
            _commandsList.Dispose();
            _resultsList.Dispose();
        }

        private void ApplyPreviousJob()
        {
            _activeJobHandle.Complete();
            for (var i = 0; i < _activeJobEntities.Count; i++)
            {
                var firingLineIsClear = true;
                var entityCastData = _activeJobEntities[i];

                if (World.IsDisposed(entityCastData.WeaponEntity))
                    continue;

                if (World.IsDisposed(entityCastData.TargetEntity))
                {
                    firingLineIsClear = false;
                }                    
                else
                {
                    var result = _resultsList[i];
                    if (result.colliderInstanceID != 0)
                    {
                        // if raycast hit actual target or at least hit enemy:
                        firingLineIsClear =
                            _collidersTable.TryGetColliderOwner(result.colliderInstanceID, out var colliderOwnerEntity)
                            && (
                                colliderOwnerEntity == entityCastData.TargetEntity
                                || _affinityHandler.AreEntitiesHostile(_affinityHandler.GetEntityOwner(entityCastData.WeaponEntity), entityCastData.TargetEntity)
                            );

                       //if (firingLineIsClear)  
                       //     UnityEngine.Debug.Log($"entity {entityCastData.WeaponEntity.Id} : {colliderOwnerEntity}");
                       //else
                       //     UnityEngine.Debug.Log("no contact");
                    }
                }               

                if (firingLineIsClear)
                    _fireLineClearTag.Set(entityCastData.WeaponEntity);
                else
                    _fireLineClearTag.Remove(entityCastData.WeaponEntity);
            }

            _activeJobEntities.Clear();
            _commandsList.Clear();
            _resultsList.Clear();
        }

        private void PrepareAndLaunchJob()
        {
            foreach (var entityCastData in _activeJobEntities)
            {
                var shotPoint = _weaponShotPoints.Get(entityCastData.WeaponEntity).WorldPoint;
                var range = _weaponRanges.Get(entityCastData.WeaponEntity).MaxRange;

                _commandsList.Add(new RaycastCommand(from: shotPoint.pos, direction: math.forward(shotPoint.rot), _queryParameters, range));
                _resultsList.Add(default);
            }

            // Schedule the batch of raycasts.
            _activeJobHandle = RaycastCommand.ScheduleBatch(_commandsList.AsArray(), _resultsList.AsArray(), minCommandsPerJob: 64, maxHits: 1);
            _isJobActive = true;
        }
    }
}