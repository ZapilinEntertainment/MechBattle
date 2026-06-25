using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TrianglePathDefineSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _regularHexPathUsers;
        private Filter _noHexPathUsers;

        private Stash<TrianglePathSearchRequestComponent> _regularPathSearchRequests;
        private Stash<TriangularPosComponent> _triangularPos;
        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<RegularTrianglePathProcessingTag> _regularProcessingTags;
        private Stash<FlowTrianglePathProcessingTag> _flowProcessingTags;

        private Stash<HexPathIdComponent> _hexPathComponents;
        private Stash<HexPathProgressionComponent> _hexPathProgressionComponents;
        private Stash<ClearHexPathTag> _invalidHexPaths;
        private Stash<TrianglePathDefinedTag> _trianglePathDefinedTag;

        private Stash<FlowMapSearchRequestComponent> _flowMapSearchRequests;
        
        private readonly HexPortalPathsLRUBuffer _hexPaths;
       
        public TrianglePathDefineSystem(HexPortalPathsLRUBuffer hexPaths) 
        {
            _hexPaths = hexPaths;
        }

        public void OnAwake() 
        {
            _regularHexPathUsers = World.Filter
                .With<HexPathReadyTag>()
                .With<HexPathProgressionComponent>()
                .Without<TrianglePathDefinedTag>()
                .Build();

            _noHexPathUsers = World.Filter
                .With<HexPathReadyTag>()
                .Without<HexPathIdComponent>()
                .Without<TrianglePathDefinedTag>()
                .Build();

            _regularPathSearchRequests = World.GetStash<TrianglePathSearchRequestComponent>();
            _triangularPos = World.GetStash<TriangularPosComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _regularProcessingTags = World.GetStash<RegularTrianglePathProcessingTag>();
            _flowProcessingTags = World.GetStash<FlowTrianglePathProcessingTag>();
            _trianglePathDefinedTag = World.GetStash<TrianglePathDefinedTag>();

            _hexPathComponents = World.GetStash<HexPathIdComponent>();
            _hexPathProgressionComponents = World.GetStash<HexPathProgressionComponent>();
            _invalidHexPaths = World.GetStash<ClearHexPathTag>();

            _flowMapSearchRequests = World.GetStash<FlowMapSearchRequestComponent>();
        }

        private struct TrianglePathStrategy
        {
            public bool BuildTrianglePath;
            public HexEdge TargetEdge;
            public IntTriangularPos TargetPos;
        }

        public void OnUpdate(float deltaTime) 
        {
            // a. move inside hex
            foreach (var entity in _noHexPathUsers)
            {
                SetupPathToFinalTarget(entity);              
                
#if ZE_NAVIGATION_DEBUG
                if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.EntityPortalPathStatuses))
                    UnityEngine.Debug.Log($"entity {entity.Id}: move inside hex");
#endif
            }

            // b. move through hex portals
            foreach (var entity in _regularHexPathUsers)
            {
                var hexPathId = _hexPathComponents.Get(entity).PathId;
                if (!_hexPaths.TryGetValue(hexPathId, out var path, updateUsingTime: false))
                {
                    _invalidHexPaths.Add(entity);
                    UnityEngine.Debug.Log("invalid hex path - no path found");
                    continue;
                }

                var progressionComponent = _hexPathProgressionComponents.Get(entity);
                var stepIndex = progressionComponent.StepIndex; 
                if (stepIndex >= progressionComponent.StepsCount)
                {
                    // all portals passed, move to final target

#if ZE_NAVIGATION_DEBUG
                    if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.EntityPortalPathStatuses))
                        UnityEngine.Debug.Log($"entity {entity.Id}: move to final target");
#endif
                    SetupPathToFinalTarget(entity);
                    continue;
                }

                if (!path.TryGetNode(stepIndex, out var portalId))
                {
                    _invalidHexPaths.Add(entity);
                    UnityEngine.Debug.Log("invalid hex path - no portal found");
                    continue;
                }

#if ZE_NAVIGATION_DEBUG
                if (NavigationLogger.Settings.HasFlag(NavigationLogEvents.FlowMapRequest))
                    UnityEngine.Debug.Log($"flow map requested for portal {portalId} index {progressionComponent.StepIndex} at {entity.GetComponent<HexCoordComponent>().Value}");
#endif

                _flowMapSearchRequests.Add(entity, new(portalId));
                _flowProcessingTags.Add(entity);
                _trianglePathDefinedTag.Add(entity);
                
            }
        }

        public void Dispose() { }

        private void SetupPathToFinalTarget(Entity entity)
        {
            var startTripos = _triangularPos.Get(entity).Value;
            var endTripos = _moveTargets.Get(entity).TriangularPos;

            _regularPathSearchRequests.Set(entity, new(startTripos, endTripos));
            _regularProcessingTags.Add(entity);
            _trianglePathDefinedTag.Add(entity);
        }
    }
}