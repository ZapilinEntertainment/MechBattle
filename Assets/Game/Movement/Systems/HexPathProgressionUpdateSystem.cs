using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathProgressionUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;

        private Stash<HexPathIdComponent> _hexPaths;
        private Stash<HexPathProgressionComponent> _hexProgression;
        private Stash<ClearHexPathTag> _clearHexPathTags;
        private Stash<ClearTrianglePathTag> _clearTrianglePathTags;
        private Stash<TriangularPosComponent> _triangularPos;
        private Stash<MoveTargetComponent> _moveTarget;
        private Stash<HexCoordComponent> _hexCoords;

        private readonly HexPortalPathsLRUBuffer _portalPaths;
        private readonly NavigationGridHandler _gridHandler;

        [Inject]
        public HexPathProgressionUpdateSystem(HexPortalPathsLRUBuffer portalsPaths, NavigationGridHandler gridHandler)
        {
            _portalPaths = portalsPaths;
            _gridHandler = gridHandler;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<CompletedTrianglePathTag>()
                .With<HexPathProgressionComponent>()
                .Build();

            _hexPaths = World.GetStash<HexPathIdComponent>();
            _hexProgression = World.GetStash<HexPathProgressionComponent>();
            _clearHexPathTags = World.GetStash<ClearHexPathTag>();
            _clearTrianglePathTags = World.GetStash<ClearTrianglePathTag>();
            _triangularPos = World.GetStash<TriangularPosComponent>();
            _moveTarget = World.GetStash<MoveTargetComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var pathId = _hexPaths.Get(entity).PathId;
                if (!_portalPaths.TryGetPathById(pathId, out var path))
                {
                    _clearHexPathTags.Add(entity);
                    continue;
                }

                ref var progression = ref _hexProgression.Get(entity);
                var hexCoord = _hexCoords.Get(entity).Value;
                if (math.any(hexCoord != progression.TargetHexCoord))
                {
#if ZE_NAVIGATION_DEBUG
                    if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.EntityLosePath))
                        UnityEngine.Debug.Log($"entity {entity.Id} lose its path: {pathId}");
#endif

                    // deviated from route
                    _clearHexPathTags.Add(entity);
                    continue;
                }


                var currentStep = progression.StepIndex;

                #if ZE_NAVIGATION_DEBUG
                if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.TripathProgression))
                    UnityEngine.Debug.Log($"completed tripath: {currentStep} / {progression.StepsCount}");
                #endif

                if (currentStep + 1 > progression.StepsCount)
                {
                    if (IsEntityReachedTarget(entity)) 
                        ClearHexPathData(entity);
                }
                else
                {
                    progression.StepIndex = currentStep + 1;

                    progression.TargetHexCoord =
                        path.TryGetNode(progression.StepIndex, out var pathNode)
                        ? _gridHandler.GetTargetHexCoord(entity, pathNode)
                        : path.End.HexCoord;
                }

                ClearTrianglePathData(entity);
            }
        }

        public void Dispose() { }

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
        }
    }
}