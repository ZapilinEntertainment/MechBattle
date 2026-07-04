using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
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

        public void OnAwake() 
        {
            _filter = World.Filter.With<ChangeMoveTargetRequestComponent>().Build();

            _requests = World.GetStash<ChangeMoveTargetRequestComponent>();
            _moveTargets = World.GetStash<MoveTargetComponent>();
            _clearHexPathTags = World.GetStash<ClearHexPathTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                _clearHexPathTags.Set(entity);

                var request = _requests.Get(entity);
                _moveTargets.Set(entity, new(request.WorldPos, request.Tripos, request.HexCoord));
                _requests.Remove(entity);
            }
        }

        public void Dispose() { }
    }
}