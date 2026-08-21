using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TargetStepPositionCheckSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<StepTargetPointComponent> _stepTargets;
        private Stash<RotationComponent> _rotations;
        private Stash<NextStepPositionCalculationRequest> _calculationRequests;
        private Stash<InvalidTargetStepPositionTag> _invalidPositionTags;

        private readonly IMechStepsMap _mechStepsMap;
        private readonly INavigationMap _navigationMap;
        
        private readonly List<OrientedPoint> _orientedPointsList = new(INITIAL_CAPACITY);
        private readonly List<float> _heightsList = new(INITIAL_CAPACITY);
        private const int INITIAL_CAPACITY = 32;


        [Inject]
        public TargetStepPositionCheckSystem(
            IMechStepsMap affectionMap, 
            INavigationMap navigationMap)
        {
            _mechStepsMap = affectionMap;
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
                // var rot = GetChassisRotation(entity, plane);                
                stepTarget.Value = new(stepTarget.Value.rot, pos);
                //UnityEngine.Debug.Log($"next point calculated on entity {entity.Id}: {pos}, plane: {plane}");
            }

            _calculationRequests.RemoveAll();
        }

        public void Dispose() { }

        

        private bool TryFormNextStepPlane(Entity entity, out float4 plane)
        {
            plane = default;
            if (!_mechStepsMap.TryGetAffectionData(entity, out var affectionData))
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
            foreach (var kvp in _mechStepsMap)
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
    }
}