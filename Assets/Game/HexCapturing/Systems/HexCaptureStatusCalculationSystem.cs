using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexCaptureStatusCalculationSystem : PausableSystem 
    {
        private Filter _captureUnitsFilter;
        private Filter _hexEntitiesFilter;

        private Stash<HexCoordComponent> _hexCoords;
        private Stash<PlayerAffiliationComponent> _affiliations;
        private Stash<CapturePowerComponent> _capturePowerComponents;

        private Stash<HexCaptureStateComponent> _hexCaptureState;
        private Stash<HexComponent> _hexComponents;

        private int _nextCoordIndex = 0;
        private readonly IPlayersList _playersList;
        private readonly Dictionary<int2, int> _hexIndices = new(128);
        private readonly List<float> _controlValues = new(1024);

        [Inject]
        public HexCaptureStatusCalculationSystem(SceneFlagsManager flags, IPlayersList playersList) : base(flags)
        {
            _playersList = playersList;
        }

        public override void OnAwake()
        {
            _captureUnitsFilter = World.Filter
                .With<CapturePowerComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _hexCoords = World.GetStash<HexCoordComponent>();
            _affiliations = World.GetStash<PlayerAffiliationComponent>();
            _capturePowerComponents = World.GetStash<CapturePowerComponent>();



            _hexEntitiesFilter = World.Filter
                .With<HexComponent>()
                .Build();

            _hexCaptureState = World.GetStash<HexCaptureStateComponent>();
            _hexComponents = World.GetStash<HexComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused || _captureUnitsFilter.IsEmpty() || _hexEntitiesFilter.IsEmpty())
                return;

            var playersCount = _playersList.Count;

            foreach (var unit in _captureUnitsFilter)
            {
                var hexCoord = _hexCoords.Get(unit).Value;
                var playerKey = _affiliations.Get(unit).PlayerKey;
                var capturePower = _capturePowerComponents.Get(unit).Value;

                if (!_hexIndices.TryGetValue(hexCoord, out var hexIndex))
                {
                    hexIndex = _nextCoordIndex++;
                    _hexIndices.Add(hexCoord, hexIndex);

                    for (var i = 0; i < playersCount; i++)
                    {
                        _controlValues.Add(0f);
                    }
                }

                var readIndex = hexIndex * 8;
                _controlValues[readIndex + playerKey.Id] += capturePower;
            }

            foreach (var hexEntity in _hexEntitiesFilter)
            {
                var hexCoord = _hexComponents.Get(hexEntity).Coord;
            }
        }
    }
}