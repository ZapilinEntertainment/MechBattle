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
        private readonly struct CachedFootAffectionData
        {
            public readonly FootAffectionData AffectionData;
            public readonly int CapacityVolume;

            public CachedFootAffectionData(FootAffectionData affectionData, int capacity)
            {
                AffectionData = affectionData;
                CapacityVolume = capacity;
            }
        }

        public World World { get; set;}
        private Filter _filter;
        private Stash<NextStepPositionCalculationRequest> _requests;       
        private Stash<MechInputComponent> _input;        
        private Stash<MechChassisComponent> _chassisComponents;

        private Stash<PositionComponent> _positions;
        private Stash<RotationComponent> _rotations;
        private Stash<InitialLocalPosition> _initLocalPositions;

        private Stash<StepTargetPointComponent> _stepTargets;
        private Stash<ChassisSettingsComponent> _stepSettings;

        private JobHandle _activeHandle;

        private readonly INavigationMap _map;
        private readonly NativeList<FootAffectionData> _footAffectionData;
        private readonly NativeList<MechStepOccupationData> _mechStepsAffectedCells;
        private readonly Dictionary<float2, CachedFootAffectionData> _footSizeCache = new();

        [Inject]
        public NextStepCellsCalculationSystem(INavigationMap map)
        {
            _map = map;
            _footAffectionData = new(initialCapacity: 32,Allocator.Persistent);
            _mechStepsAffectedCells = new(Allocator.Persistent);
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<NextStepPositionCalculationRequest>()
                .With<MechInputComponent>()
                .Build();

            _requests = World.GetStash<NextStepPositionCalculationRequest>();
            
            _input = World.GetStash<MechInputComponent>();            
            _chassisComponents = World.GetStash<MechChassisComponent>();

            _initLocalPositions = World.GetStash<InitialLocalPosition>();
            _positions = World.GetStash<PositionComponent>();
            _rotations = World.GetStash<RotationComponent>();

            _stepTargets = World.GetStash<StepTargetPointComponent>();
            _stepSettings = World.GetStash<ChassisSettingsComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            #region Prepare native data
            var nativeFilter = _filter.AsNative();
            var chassisComponents = _chassisComponents.AsNative();
            var stepTargets = _stepTargets.AsNative();
            var stepSettings = _stepSettings.AsNative();
            var requests = _requests.AsNative();

            PrepareCapacities(nativeFilter);
            #endregion

            var nextPosJobHandle = new DefineFootNextPositionJob()
            {
                Filter = nativeFilter,
                Input = _input.AsNative(),

                LocalPositions = _initLocalPositions.AsNative(),
                Positions = _positions.AsNative(),
                Rotations = _rotations.AsNative(),

                StepSettings = stepSettings,
                StepTargets = stepTargets,
                Requests = requests
                
            }.Schedule(nativeFilter.length, 4);

            _activeHandle = new GetStepAffectedTrianglesJob()
            {                
                Filter = nativeFilter,
                StepTargets = stepTargets,

                TriangleHeight = _map.TriangleHeight,
                FootAffectionData = _footAffectionData,
                StepAffectedCells = _mechStepsAffectedCells.AsParallelWriter()                
            }.Schedule(nativeFilter.length, 4, dependsOn: nextPosJobHandle);

            World.JobHandle = _activeHandle;

            //UnityEngine.Debug.Log("target jobs launched");
        }

        public void Dispose() 
        {
#if UNITY_EDITOR
            try
            {
#endif
                _mechStepsAffectedCells.Dispose();
                _footAffectionData.Dispose();
#if UNITY_EDITOR
            }
            catch
            {

            }
#endif
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

        private void PrepareCapacities(NativeFilter filter)
        {
            _mechStepsAffectedCells.Clear();
            _footAffectionData.Clear();

            var maxCount = 0;
            for (var i = 0; i < filter.length; i++)
            {
                var entity = filter[i];

                var footSize = _stepSettings.Get(entity).FootSize;
                if (_footSizeCache.TryGetValue(footSize, out var cachedData))
                {
                    maxCount += cachedData.CapacityVolume;
                    _footAffectionData.Add(cachedData.AffectionData);
                    continue;
                }

                var rectOutlinedRadius = math.sqrt(footSize.x * footSize.x + footSize.y + footSize.y) * 0.5f;
                var hexOutlinedRadius = MathExtensions.UnitsRadiusToTriangular(rectOutlinedRadius, _map.TriangleHeight);
                var affectionData = new FootAffectionData(footSize, hexOutlinedRadius);
                var maxTrisCount = TriangularMath.GetTrianglesCountInHex(hexOutlinedRadius);

                _footAffectionData.Add(affectionData);
                _footSizeCache.Add(footSize, new(affectionData, maxTrisCount));

                maxCount += maxTrisCount;
            }

            if (_mechStepsAffectedCells.Capacity < maxCount)
                _mechStepsAffectedCells.SetCapacity(maxCount);
        }
    }
}