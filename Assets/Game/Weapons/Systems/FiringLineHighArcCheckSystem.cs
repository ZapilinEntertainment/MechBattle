using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using ZE.MechBattle.Weapons;
using Unity.Jobs;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FiringLineHighArcCheckSystem : FiringLineCheckSystemBase<CalculateFireLineByHighArcTag>
    {
        private readonly NativeList<Entity> _calculatingEntitiesList;

        private Stash<HighAimResultsComponent> _results;
        private Stash<ProjectileSpeedComponent> _projectileSpeed;
        private Stash<WeaponShotPoint> _shotPoints;      
        private readonly float _gravity;
        private const Allocator ALLOCATOR = Allocator.Persistent;


        public FiringLineHighArcCheckSystem(CollidersTable collidersTable, AffinityHandler affinityHandler, SceneFlagsManager flags) : base(collidersTable, affinityHandler, flags)
        {
            _calculatingEntitiesList = new(16, ALLOCATOR);
            _gravity = -Physics.gravity.y;
        }

        public override void OnAwake()
        {
            base.OnAwake();

            _results = World.GetStash<HighAimResultsComponent>();
            _projectileSpeed = World.GetStash<ProjectileSpeedComponent>();
            _shotPoints = World.GetStash<WeaponShotPoint>();
        }

        protected override void PrepareAndLaunchJob()
        {
            foreach (var entityCastData in _activeJobEntities)
            {
                _calculatingEntitiesList.Add(entityCastData.WeaponEntity);               
            }

            var elementsCount = _activeJobEntities.Count;
            _commandsList.ResizeUninitialized(elementsCount * HighArcRaycastSegments.SEGMENTS_COUNT);
            _resultsList.ResizeUninitialized(elementsCount * HighArcRaycastSegments.SEGMENTS_COUNT);

            var preparationJob = new HighAngleFirelineCheckPreparationJob()
            {
                Gravity = _gravity,
                AimResults = _results.AsNative(),
                ProjectileSpeed = _projectileSpeed.AsNative(),
                ShotPoints = _shotPoints.AsNative(),
                WeaponEntity = _calculatingEntitiesList.AsReadOnly(),
                RaycastInput = _commandsList.AsArray()                
            };

            var preparationHandle = preparationJob.Schedule(elementsCount, 32);
            _activeJobHandle = RaycastCommand.ScheduleBatch(_commandsList.AsArray(), _resultsList.AsArray(), minCommandsPerJob: 64, maxHits: 1, dependsOn : preparationHandle);
            _isJobActive = true;
        }

        protected override void ApplyPreviousJob()
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
                    var resultIndex = i * HighArcRaycastSegments.SEGMENTS_COUNT;

                    firingLineIsClear = 
                        IsFirelineClear(resultIndex, entityCastData.WeaponEntity, entityCastData.TargetEntity)
                        && IsFirelineClear(resultIndex + 1, entityCastData.WeaponEntity, entityCastData.TargetEntity)
                        && IsFirelineClear(resultIndex + 2, entityCastData.WeaponEntity, entityCastData.TargetEntity);
                }

                if (firingLineIsClear)
                    _fireLineClearTag.Set(entityCastData.WeaponEntity);
                else
                    _fireLineClearTag.Remove(entityCastData.WeaponEntity);
            }
        }
       

        protected override FilterBuilder GetFilterBuilder() =>
            base
            .GetFilterBuilder()
            .With<WeaponShotPoint>()
            .With<ProjectileSpeedComponent>()
            .With<HighAimResultsComponent>();

        protected override void InternalDispose()
        {
            base.InternalDispose();
            _calculatingEntitiesList.Dispose();
        }

        protected override void OnJobApplied()
        {
            base.OnJobApplied();
            _calculatingEntitiesList.Clear();
        }
    }
}