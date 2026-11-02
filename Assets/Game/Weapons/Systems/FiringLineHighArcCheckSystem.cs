using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;
using ZE.MechBattle.Weapons;
using Unity.Jobs;
using ZE.Utils;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class FiringLineHighArcCheckSystem : FiringLineCheckSystemBase<CalculateFireLineByHighArcTag>
    {
        private readonly NativeList<Entity> _calculatingEntitiesList;

        private Stash<HighAimResultsComponent> _aimResults;
        private Stash<ProjectileSpeedComponent> _projectileSpeed;
        private Stash<WeaponShotPoint> _shotPoints;      
        private readonly float _gravity;


        public FiringLineHighArcCheckSystem(CollidersTable collidersTable, AffinityHandler affinityHandler, SceneFlagsManager flags) : base(collidersTable, affinityHandler, flags)
        {
            _calculatingEntitiesList = new(16, ALLOCATOR);
            _gravity = -Physics.gravity.y;
        }

        public override void OnAwake()
        {
            base.OnAwake();

            _aimResults = World.GetStash<HighAimResultsComponent>();
            _projectileSpeed = World.GetStash<ProjectileSpeedComponent>();
            _shotPoints = World.GetStash<WeaponShotPoint>();
        }

        protected override void PrepareAndLaunchJob()
        {
            foreach (var entityCastData in _activeJobEntities)
            {
                _calculatingEntitiesList.Add(entityCastData.WeaponEntity);               
            }

            var weaponsCount = _activeJobEntities.Count;
            var checksCount = weaponsCount * HighArcRaycastSegments.SEGMENTS_COUNT;
            _raycastCommands = ExtendNativeCollectionCommand.ExecuteIfNeeded(_raycastCommands, checksCount, ALLOCATOR);
            _results = ExtendNativeCollectionCommand.ExecuteIfNeeded(_results, checksCount, ALLOCATOR);

            var preparationJob = new HighAngleFirelineCheckPreparationJob()
            {
                Gravity = _gravity,
                AimResults = _aimResults.AsNative(),
                ProjectileSpeed = _projectileSpeed.AsNative(),
                ShotPoints = _shotPoints.AsNative(),
                WeaponEntity = _calculatingEntitiesList.AsReadOnly(),
                RaycastInput = _raycastCommands,
                QueryParameters = _queryParameters
            };

            var preparationHandle = preparationJob.Schedule(weaponsCount, 32);
            _activeJobHandle = RaycastCommand.ScheduleBatch(_raycastCommands.GetSubArray(0, checksCount), _results.GetSubArray(0, checksCount), minCommandsPerJob: 64, maxHits: 1, dependsOn : preparationHandle);
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

                    //UnityEngine.Debug.Log($"{IsFirelineClear(resultIndex, entityCastData.WeaponEntity, entityCastData.TargetEntity)} : {IsFirelineClear(resultIndex + 1, entityCastData.WeaponEntity, entityCastData.TargetEntity)} : {IsFirelineClear(resultIndex + 2, entityCastData.WeaponEntity, entityCastData.TargetEntity)}");

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