using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {

    public interface IMechStepsAffectionMap
    {
        void GetStepAffectedCells(Action<IntTriangularPos, Entity> addAffectionData);
    }


    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class NextStepCellsCalculationSystem : ISystem, IMechStepsAffectionMap
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<CalculateNextFootPositionRequestComponent> _requests;       
        private Stash<MechInputComponent> _input;        
        private Stash<MechChassisComponent> _chassisComponents;

        private Stash<PositionComponent> _positions;
        private Stash<RotationComponent> _rotations;
        private Stash<InitialLocalPosition> _initLocalPositions;

        private Stash<StepTargetPointComponent> _stepTargets;
        private Stash<StepProgressionComponent> _stepProgression;
        private Stash<ChassisSettingsComponent> _stepSettings;

        private JobHandle _activeHandle;

        private readonly INavigationMap _map;
        private readonly NativeList<MechStepOccupationData> _mechStepsAffectedCells;
        private readonly Dictionary<float2, int> _footSizeCache = new();

        [Inject]
        public NextStepCellsCalculationSystem(INavigationMap map)
        {
            _map = map;
            _mechStepsAffectedCells = new(Allocator.Persistent);
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<CalculateNextFootPositionRequestComponent>()
                .With<MechInputComponent>()
                .Build();

            _requests = World.GetStash<CalculateNextFootPositionRequestComponent>();
            
            _input = World.GetStash<MechInputComponent>();            
            _chassisComponents = World.GetStash<MechChassisComponent>();

            _initLocalPositions = World.GetStash<InitialLocalPosition>();
            _positions = World.GetStash<PositionComponent>();
            _rotations = World.GetStash<RotationComponent>();

            _stepTargets = World.GetStash<StepTargetPointComponent>();
            _stepSettings = World.GetStash<ChassisSettingsComponent>();
            _stepProgression = World.GetStash<StepProgressionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            _requests.AsNative();

            foreach (var chassisEntity in _filter)
            {
                _stepTargets.Set(chassisEntity);
            }
            World.Commit();

            var nativeFilter = _filter.AsNative();
            var chassisComponents = _chassisComponents.AsNative();
            var stepProgressions = _stepProgression.AsNative();
            var stepTargets = _stepTargets.AsNative();
            var stepSettings = _stepSettings.AsNative();

            var nextPosJobHandle = new DefineFootNextPositionJob()
            {
                Filter = nativeFilter,
                ChassisComponents = chassisComponents,
                Input = _input.AsNative(),

                LocalPositions = _initLocalPositions.AsNative(),
                Positions = _positions.AsNative(),
                Rotations = _rotations.AsNative(),

                StepProgression = stepProgressions,
                StepSettings = stepSettings,
                StepTargets = stepTargets
            }.Schedule(nativeFilter.length, 4);

            CheckAffectedCellsCapacity();

            _activeHandle = new GetStepAffectedTrianglesJob()
            {
                Filter = nativeFilter,
                ChassisSettingsComponents = stepSettings,
                StepProgressions = stepProgressions,
                StepTargets = stepTargets,

                TriangleHeight = _map.TriangleHeight,
                StepAffectedCells = _mechStepsAffectedCells.AsParallelWriter()
            }.Schedule(nativeFilter.length, 4, dependsOn: nextPosJobHandle);

            World.JobHandle = _activeHandle;
        }

        public void Dispose() 
        {
            _mechStepsAffectedCells.Dispose();
        }

        public void GetStepAffectedCells(Action<IntTriangularPos, Entity> addAffectionData )
        {
            if (!_activeHandle.IsCompleted)
            {
                UnityEngine.Debug.LogWarning("active handle was not completed! Ensure you receive data from other system group");
                _activeHandle.Complete();
            }

            foreach (var data in _mechStepsAffectedCells)
            {
                addAffectionData(data.Tripos, data.Entity);
            }
        }

        private void CheckAffectedCellsCapacity()
        {
            var maxCount = 0;
            foreach (var chassisEntity in _filter)
            {
                var footSize = _stepSettings.Get(chassisEntity).FootSize;
                if (_footSizeCache.TryGetValue(footSize, out var count))
                {
                    maxCount += count;
                    continue;
                }

                var leftLegTurn = _stepProgression.Get(chassisEntity).LeftLegTurn;
                var chassisComponent = _chassisComponents.Get(chassisEntity);
                var footEntity = leftLegTurn ? chassisComponent.LeftLeg.Foot : chassisComponent.RightLeg.Foot;
                var footPos = _positions.Get(footEntity).Value;
                var footRot = _rotations.Get(footEntity).Value;

                MathExtensions.ComputeAABB(footSize, footPos, footRot, out var min, out var max);
                var width = max.x - min.x;
                var length = max.z - min.z;

                var trisWidth = (int)math.ceil(width / _map.TriangleEdgeSize);
                var trisLength = (int)math.ceil(length / _map.TriangleEdgeSize);
                trisWidth = TriangularMath.GetTwoRowEdgeTrianglesCount(trisWidth);

                var resultingMaxCount = trisLength * trisWidth;
                _footSizeCache.Add(footSize, resultingMaxCount);
                maxCount += resultingMaxCount;
            }

            if (_mechStepsAffectedCells.Capacity < maxCount)
                _mechStepsAffectedCells.SetCapacity(maxCount);
        }


        private RigidTransform AdjustNextStepAccordingToHeight(float3 targetFootPos, float3 moveVectorLocal, Entity movingLeg)
        {
            return default;
            // todo: do interface note if height is not reachable

            //var currentLegPoint = _transformAspectHandler.GetPoint(movingLeg);
            //if (!_groundCaster.TryGetGroundPoint(targetFootPos.x, targetFootPos.z, out var point))
            //    return currentLegPoint;

            //var deltaHeight = leg.DefaultFootLocalPosition.y - _chassis.Transform.InverseTransformPoint(point.Position).y;

            //if (math.abs(deltaHeight) > MaxHeightDelta)
            //{
            //    var startFootPos = currentLegPoint.pos;
            //    var projectedStart = new float2(startFootPos.x, startFootPos.z);
            //    var projectedEnd = new float2(targetFootPos.x, targetFootPos.z);
            //    var pos = math.lerp(projectedStart, projectedEnd, 0.5f);
            //    if (math.lengthsq(projectedStart - pos) < _chassis.HipsDistance * _chassis.HipsDistance * 0.25f)
            //        return currentLegPoint;

            //    return AdjustNextStepAccordingToHeight(new Vector3(pos.x, 0f, pos.y), moveVectorLocal, leg);
            //}
            //return new(Quaternion.LookRotation(transform.TransformDirection(moveVectorLocal), point.Normal), point.Position);
        }
    }
}