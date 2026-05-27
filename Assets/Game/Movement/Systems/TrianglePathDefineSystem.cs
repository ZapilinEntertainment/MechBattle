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

        private Stash<HexPathComponent> _hexPathComponents;
        private Stash<HexPathProgressionComponent> _hexPathProgressionComponents;
        private Stash<ClearHexPathTag> _invalidHexPaths;

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
                .Without<HexPathComponent>()
                .Without<TrianglePathDefinedTag>()
                .Build();

            _regularPathSearchRequests = World.GetStash<TrianglePathSearchRequestComponent>();
            _triangularPos = World.GetStash<TriangularPosComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _regularProcessingTags = World.GetStash<RegularTrianglePathProcessingTag>();
            _flowProcessingTags = World.GetStash<FlowTrianglePathProcessingTag>();

            _hexPathComponents = World.GetStash<HexPathComponent>();
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
            }

            // b. move through hex portals
            foreach (var entity in _regularHexPathUsers)
            {
                var hexPathId = _hexPathComponents.Get(entity).PathId;
                if (!_hexPaths.TryGetValue(hexPathId, out var path, updateUsingTime: false))
                {
                    _invalidHexPaths.Add(entity);
                    continue;
                }

                var progressionComponent = _hexPathProgressionComponents.Get(entity);
                var stepIndex = progressionComponent.StepIndex; 
                if (stepIndex >= progressionComponent.StepsCount)
                {
                    // all portals passed, move to final target
                    SetupPathToFinalTarget(entity);
                    continue;
                }

                if (!path.TryGetNode(stepIndex, out var portalId))
                {
                    _invalidHexPaths.Add(entity);
                    continue;
                }

                _flowMapSearchRequests.Add(entity, new(portalId));
                _flowProcessingTags.Add(entity);
            }
        }

        public void Dispose() { }

        private void SetupPathToFinalTarget(Entity entity)
        {
            var startTripos = _triangularPos.Get(entity).Value;
            var endTripos = _moveTargets.Get(entity).TriangularPos;

            _regularPathSearchRequests.Set(entity, new(startTripos, endTripos));
            _regularProcessingTags.Add(entity);
        }
    }
}