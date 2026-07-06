using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ChangeMovementTargetSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<ChangeMoveTargetRequestComponent> _requests;
        private Stash<MoveTargetComponent> _moveTargets;
        private Stash<ClearHexPathTag> _clearHexPathTags;
        private Stash<ClearTrianglePathTag> _clearTrianglePathTag;
        private Stash<HexCoordComponent> _hexCoordComponents;

        public void OnAwake() 
        {
            _filter = World.Filter.With<ChangeMoveTargetRequestComponent>().Build();

            _requests = World.GetStash<ChangeMoveTargetRequestComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _clearHexPathTags = World.GetStash<ClearHexPathTag>();
            _clearTrianglePathTag = World.GetStash<ClearTrianglePathTag>();
            _hexCoordComponents = World.GetStash<HexCoordComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var moveTargetComponent = _moveTargets.Get(entity, out var hasMoveTarget);
                var request = _requests.Get(entity);
                if (!hasMoveTarget)
                {
                    ApplyRequest(entity, request);
                    continue;
                }

                
                if (math.any(moveTargetComponent.HexCoord != request.HexCoord))
                {
                    _clearHexPathTags.Set(entity);
                }
                else
                {
                    var currentHexCoord = _hexCoordComponents.Get(entity).Value;
                    // clear triangle path only if in final hex (otherwise it will be calculated after arrival into)
                    if (math.all(currentHexCoord == request.HexCoord) && moveTargetComponent.TriangularPos != request.Tripos)
                        _clearTrianglePathTag.Set(entity);
                }              

                ApplyRequest(entity, request);
            }
        }

        public void Dispose() { }

        private void ApplyRequest(Entity entity, ChangeMoveTargetRequestComponent request)
        {
            _moveTargets.Set(entity, new(request.WorldPos, request.Tripos, request.HexCoord));
            _requests.Remove(entity);
        }
    }
}