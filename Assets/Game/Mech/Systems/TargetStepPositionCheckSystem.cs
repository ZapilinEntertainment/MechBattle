using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;
using ZE.MechBattle.MechMovement;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TargetStepPositionCheckSystem : ISystem 
    {
        private struct StepAffectionData
        {
            public int TotalCellsCount;
            public int SuitableCellsCount;
        }

        public World World { get; set;}
        private Filter _filter;
        private Stash<StepTargetPointComponent> _stepTargets;
        private Stash<RotationComponent> _rotations;
        private Stash<NextStepPositionCalculationRequest> _calculationRequests;
        private Stash<InvalidTargetStepPositionTag> _invalidPositionTags;

        private readonly IMechStepsAffectionMap _affectionMapSource;
        private readonly INavigationMap _navigationMap;
        private readonly Dictionary<Entity, StepAffectionData> _affectionData = new(INITIAL_CAPACITY);
        private readonly Dictionary<IntTriangularPos, Entity> _alreadyOccupiedCells = new(INITIAL_CAPACITY);
        private readonly List<OrientedPoint> _orientedPointsList = new(INITIAL_CAPACITY);
        private readonly List<float> _heightsList = new(INITIAL_CAPACITY);
        private const int INITIAL_CAPACITY = 32;

        [Inject]
        public TargetStepPositionCheckSystem(
            IMechStepsAffectionMap affectionMap, 
            INavigationMap navigationMap)
        {
            _affectionMapSource = affectionMap;
            _navigationMap = navigationMap;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<NextStepPositionCalculationRequest>()
                .Build();

            _stepTargets = World.GetStash<StepTargetPointComponent>();
            _rotations = World.GetStash<RotationComponent>();
            _calculationRequests = World.GetStash<NextStepPositionCalculationRequest>();
            _invalidPositionTags = World.GetStash<InvalidTargetStepPositionTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            _affectionMapSource.GetStepAffectedCells(AddAffectionData);
            foreach (var entity in _filter)
            {
                if (!TryFormNextStepPlane(entity, out var plane))
                {
                    UnityEngine.Debug.Log("target plane not exists");
                    _invalidPositionTags.Set(entity);
                    continue;
                }                   

                ref var stepTarget = ref _stepTargets.Get(entity);
                var pos = MathExtensions.ProjectPointOnPlane(stepTarget.Value.pos, plane);
                var rot = GetChassisRotation(entity, plane);                

                stepTarget.Value = new(rot, pos);
                //UnityEngine.Debug.Log($"next point calculated: {pos}, plane: {plane}");
            }

            _affectionData.Clear();
            _alreadyOccupiedCells.Clear();

            _calculationRequests.RemoveAll();
        }

        public void Dispose() { }

        private void AddAffectionData(IntTriangularPos tripos, Entity entity)
        {
            _affectionData.TryGetValue(entity, out var affectionData);
            affectionData.TotalCellsCount++;

            if (_alreadyOccupiedCells.TryAdd(tripos, entity))
                affectionData.SuitableCellsCount++;

            _affectionData[entity] = affectionData;
        }

        private bool TryFormNextStepPlane(Entity entity, out float4 plane)
        {
            plane = default;
            if (!_affectionData.TryGetValue(entity, out var affectionData))
            {
                UnityEngine.Debug.LogError("affection data not found");
                return false;
            }

            var acceptableCellsPc = affectionData.SuitableCellsCount / (float)affectionData.TotalCellsCount;
            if (acceptableCellsPc < MechConstants.MIN_SUITABLE_CELLS_PC)
            {
               // UnityEngine.Debug.Log($"suitable cells: {affectionData.SuitableCellsCount} / {affectionData.TotalCellsCount}");
                return false;
            }
                

            _orientedPointsList.Clear();
            _heightsList.Clear();
            foreach (var kvp in _alreadyOccupiedCells)
            {
                if (kvp.Value == entity)
                {
                    var tripos = kvp.Key;
                    var heightData = _navigationMap.GetHeightData(tripos);
                    var orientedPoint = CalculateTriangleOrientedPoint.Execute(tripos, _navigationMap.TriangleHeight, heightData);
                    _orientedPointsList.Add(orientedPoint);
                    _heightsList.Add(heightData.AverageHeight);
                }                    
            }
            
            return CalculateRansacPlaneCommand.TryGetBestPlane(
                _orientedPointsList,
                _heightsList,
                NavigationConstants.GetRansacIterationsCount(affectionData.SuitableCellsCount),
                ransacThreshold: MechConstants.MAX_TARGET_POS_HEIGHT_ABERRATION,
                out plane);
        }

        private quaternion GetChassisRotation(Entity entity, float4 plane)
        {
            var chassisEntity = _calculationRequests.Get(entity).ChassisEntity;
            var chassisRotation = _rotations.Get(entity).Value;
            var chassisForward = math.mul(chassisRotation, math.forward());
            return quaternion.LookRotation(chassisForward, plane.xyz);
        }
    }
}