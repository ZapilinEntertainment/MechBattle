using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    // change entities owner if they are capturable and hex owner is changed
    public sealed class EntitiesCaptureSystem : PausableSystem 
    {
        private Filter _recaptureHexFilter;
        private Filter _capturableEntities;
        private Stash<HexComponent> _hexComponents;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<HexCaptureStateComponent> _hexCaptureState;
        private readonly AffinityHandler _affinityHandler;
        private readonly Dictionary<int2, int> _updatedHexList = new(8);

        public EntitiesCaptureSystem(SceneFlagsManager flags, AffinityHandler affinityHandler) : base(flags)
        {
            _affinityHandler = affinityHandler;
        }

        public override void OnAwake()
        {
            _recaptureHexFilter = World.Filter
                .With<HexOwnerChangedTag>()
                .Build();

            _capturableEntities = World.Filter
                .With<CapturableEntityTag>()
                .Without<EntityDisposeTag>()
                .Build();

            _hexComponents = World.GetStash<HexComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
            _hexCaptureState = World.GetStash<HexCaptureStateComponent>();
        }
       

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused || _recaptureHexFilter.IsEmpty())
                return;

            foreach (var hexEntity in _recaptureHexFilter)
            {
                _updatedHexList.Add(_hexComponents.Get(hexEntity).Coord, _hexCaptureState.Get(hexEntity).ControllingPlayerId);
            }

            foreach (var capturableEntity in _capturableEntities)
            {
                var coord = _hexCoords.Get(capturableEntity).Value;
                if (_updatedHexList.TryGetValue(coord, out var playerId))
                {
                    _affinityHandler.SetEntityPlayerAffinity(capturableEntity, new(playerId));
                }
                    
            }

            _updatedHexList.Clear();
        }
    }
}