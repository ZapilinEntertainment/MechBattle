using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathProgressionUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _regularHexPathWithCompletedStage;
        private Filter _otherHexPathUsersWithCompletedStage;

        private Stash<CompletedTrianglePathTag> _completedTags;
        private Stash<RegularHexPathComponent> _regularHexPaths;
        private Stash<TransitionHexPathComponent> _transitionHexPaths;

        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<TriangularPosComponent> _triangularPosComponents;
        private Stash<HexCoordComponent> _hexCoordComponents;

        private Stash<ClearHexPathTag> _hexPathClearTags;
        private Stash<ClearTrianglePathTag> _trianglePathClearTags;

        public void OnAwake() 
        {
            _regularHexPathWithCompletedStage = World.Filter
                .With<CompletedTrianglePathTag>()
                .With<RegularHexPathComponent>()
                .Build();

            _otherHexPathUsersWithCompletedStage = World.Filter
                .With<CompletedTrianglePathTag>()
                .Without<RegularHexPathComponent>()
                .Build();

            _completedTags = World.GetStash<CompletedTrianglePathTag>(); 
            _regularHexPaths = World.GetStash<RegularHexPathComponent>();
            _transitionHexPaths = World.GetStash<TransitionHexPathComponent>();

            _moveTargets = World.GetStash<MoveTargetComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
            _hexCoordComponents = World.GetStash<HexCoordComponent>();

            _hexPathClearTags = World.GetStash<ClearHexPathTag>();
            _trianglePathClearTags = World.GetStash<ClearTrianglePathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            CheckRegularHexPathUsers();
            CheckOtherUsers();
        }

        public void Dispose() { }

        private void CheckRegularHexPathUsers()
        {
            foreach (var entity in _regularHexPathWithCompletedStage)
            {
                ref var hexPathComponent = ref _regularHexPaths.Get(entity);
                var currentStep = hexPathComponent.StepIndex;
                if (currentStep + 1 > hexPathComponent.StepsCount)
                {
                    DoTargetCheck(entity);
                }
                else
                {
                    hexPathComponent.StepIndex = currentStep + 1;
                }

                ClearTrianglePathData(entity);
            }
        }

        private void CheckOtherUsers()
        {
            foreach (var entity in _otherHexPathUsersWithCompletedStage)
            {
                DoTargetCheck(entity);              
            }
        }

        private void DoTargetCheck(Entity entity)
        {
            if (IsEntityReachedTarget(entity))
            {
                ClearHexPathData(entity);
            }
            else
            {
                ClearTrianglePathData(entity);
            }
        }

        private bool IsEntityReachedTarget(Entity entity)
        {
            var target = _moveTargets.Get(entity).TriangularPos;
            var tripos = _triangularPosComponents.Get(entity).Value;
            return tripos == target;
        }

        private void ClearTrianglePathData(Entity entity)
        {
            _trianglePathClearTags.Set(entity);
            _completedTags.Remove(entity);
        }

        private void ClearHexPathData(Entity entity)
        {
            _moveTargets.Remove(entity);
            _hexPathClearTags.Add(entity);
            ClearTrianglePathData(entity);
        }
    }
}