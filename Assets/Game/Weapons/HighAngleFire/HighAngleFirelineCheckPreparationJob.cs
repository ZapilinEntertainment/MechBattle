using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Projectiles;

namespace ZE.MechBattle.Weapons
{

    // note: movement prediction must be calculated inside Aim System, not here
    public struct HighAngleFirelineCheckPreparationJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Entity>.ReadOnly WeaponEntity;
        [ReadOnly] public NativeStash<WeaponShotPoint> ShotPoints;
        [ReadOnly] public NativeStash<HighAimResultsComponent> AimResults;
        [ReadOnly] public NativeStash<ProjectileSpeedComponent> ProjectileSpeed;

        // writes in 3 cells per itertation
        [NativeDisableParallelForRestriction][WriteOnly] public NativeArray<RaycastCommand> RaycastInput;

        public float Gravity;
        public QueryParameters QueryParameters;
        private const float SLOPE_THRESHOLD  = 0.2f;

        public void Execute(int index)
        {
            var weaponEntity = WeaponEntity[index];
            var startPos = ShotPoints.Get(weaponEntity).WorldPoint.pos;
            var aimResults = AimResults.Get(weaponEntity);
            var projectileSpeed = ProjectileSpeed.Get(weaponEntity).Value;

            var projectileMoveVector = projectileSpeed * aimResults.NormalizedDirectionVector;

            // 2. Доли разбиения
            var segmentFractions = HighAngleTrajectoryPartitioningLogic.CalculatePartitions(
                projectileMoveVector, Gravity, aimResults.FlightTime, SLOPE_THRESHOLD);

            // 3. Три отрезка
            var segments = HighAngleTrajectoryRaycastPointsDefineLogic.BuildThreeSegments(
                startPos, projectileMoveVector, Gravity, aimResults.FlightTime, segmentFractions.x, segmentFractions.y, pad: 0f);

            var i = index * HighArcRaycastSegments.SEGMENTS_COUNT;

            RaycastInput[i] = segments.StartSegment.ToRaycastCommand(QueryParameters);
            RaycastInput[i + 1] = segments.MiddleSegment.ToRaycastCommand(QueryParameters);
            RaycastInput[i + 2] = segments.EndSegment.ToRaycastCommand(QueryParameters);
        }
    }
}
