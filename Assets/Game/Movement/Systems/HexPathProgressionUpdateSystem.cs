using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathProgressionUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;

        private Stash<HexPathComponent> _hexPaths;
        private Stash<HexPathProgressionComponent> _hexProgression;
        private Stash<ClearHexPathTag> _clearHexPathTags;
        private Stash<ClearTrianglePathTag> _clearTrianglePathTags;
        private Stash<TriangularPosComponent> _triangularPos;
        private Stash<MoveTargetComponent> _moveTarget;

        private readonly HexPortalsList _portalsList;

        [Inject]
        public HexPathProgressionUpdateSystem(HexPortalsList portalsList)
        {
            _portalsList = portalsList;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<CompletedTrianglePathTag>()
                .With<HexPathProgressionComponent>()
                .Build();

            _hexPaths = World.GetStash<HexPathComponent>();
            _hexProgression = World.GetStash<HexPathProgressionComponent>();
            _clearHexPathTags = World.GetStash<ClearHexPathTag>();
            _clearTrianglePathTags = World.GetStash<ClearTrianglePathTag>();
            _triangularPos = World.GetStash<TriangularPosComponent>();
            _moveTarget = World.GetStash<MoveTargetComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var pathId = _hexPaths.Get(entity).PathId;
                if (!_portalsList.ContainsKey(pathId))
                {
                    _clearHexPathTags.Add(entity);
                    continue;
                }

                ref var progression = ref _hexProgression.Get(entity);
                var currentStep = progression.StepIndex;
                if (currentStep + 1 > progression.StepsCount)
                {
                    DoTargetCheck(entity);
                }
                else
                {
                    progression.StepIndex = currentStep + 1;
                }

                ClearTrianglePathData(entity);
            }
        }

        public void Dispose() { }

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
            var target = _moveTarget.Get(entity).TriangularPos;
            var tripos = _triangularPos.Get(entity).Value;
            return tripos == target;
        }

        private void ClearTrianglePathData(Entity entity)
        {
            _clearTrianglePathTags.Set(entity);
        }

        private void ClearHexPathData(Entity entity)
        {
            _moveTarget.Remove(entity);
            _clearHexPathTags.Add(entity);
            ClearTrianglePathData(entity);
        }
    }
}