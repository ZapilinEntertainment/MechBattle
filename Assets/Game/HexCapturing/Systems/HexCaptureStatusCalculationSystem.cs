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
        private int _playersCount = 1;
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

            CalculatePlayerCaptureValues();
            UpdateHexCaptureStatuses(deltaTime);

            _hexIndices.Clear();
            _controlValues.Clear();
        }

        private void CalculatePlayerCaptureValues()
        {
            _playersCount = _playersList.Count;
            _nextCoordIndex = 0;

            foreach (var unit in _captureUnitsFilter)
            {
                var hexCoord = _hexCoords.Get(unit).Value;
                var playerKey = _affiliations.Get(unit).PlayerKey;
                var capturePower = _capturePowerComponents.Get(unit).Value;

                if (!_hexIndices.TryGetValue(hexCoord, out var hexIndex))
                {
                    hexIndex = _nextCoordIndex++;
                    _hexIndices.Add(hexCoord, hexIndex);

                    for (var i = 0; i < _playersCount; i++)
                    {
                        _controlValues.Add(0f);
                    }
                }

                var dataIndex = GetPlayerControlValueIndexByIndex(playerKey.Id, hexIndex);
                _controlValues[dataIndex] += capturePower;
            }
        }

        private int GetPlayerControlValueIndexByHexCoord(int playerId, int2 hexCoord) => GetPlayerControlValueIndexByIndex(playerId, _hexIndices[hexCoord]);
        private int GetPlayerControlValueIndexByIndex(int playerId, int hexDataIndex) => hexDataIndex * _playersCount + playerId ;

        private void UpdateHexCaptureStatuses(float deltaTime)
        {
            foreach (var hexEntity in _hexEntitiesFilter)
            {
                var hexCoord = _hexComponents.Get(hexEntity).Coord;
                var (maxPlayerId, maxPlayerControlValue) = GetMaxPlayerControlValue(hexCoord);

                ref var hexCaptureStateComponent = ref _hexCaptureState.Get(hexEntity);

                var currentOwnerExists = hexCaptureStateComponent.ControllingPlayerId != -1;
                var capturerExists = maxPlayerId != -1;

                if (!capturerExists)
                {
                    if (!currentOwnerExists)
                    {
                        // no capturer and hex was clear
                        MarkHexAsClear(ref hexCaptureStateComponent);
                    }
                    else
                    {
                        // all capturing units leave hex
                        DoDecap(ref hexCaptureStateComponent, CaptureConstants.EMPTY_HEX_DECAP_SPEED * deltaTime);
                    }
                }
                else
                {
                    if (!currentOwnerExists || hexCaptureStateComponent.ControllingPlayerId == maxPlayerId)
                    {
                        // if empty hex or player is already owns it
                        var captureStep = math.clamp(maxPlayerControlValue * CaptureConstants.CAPTURE_SPEED_CF, 0f, CaptureConstants.MAX_CAPTURE_SPEED);
                        DoCaptureProgress(ref hexCaptureStateComponent, maxPlayerId, captureStep * deltaTime);
                    }
                    else
                    {
                        // taking controls from another player
                        var decapStep = maxPlayerControlValue - _controlValues[GetPlayerControlValueIndexByHexCoord(hexCaptureStateComponent.ControllingPlayerId, hexCoord)];
                        decapStep = math.clamp(decapStep * CaptureConstants.DECAP_SPEED_CF, 0f, CaptureConstants.MAX_DECAP_SPEED);
                        DoDecap(ref hexCaptureStateComponent, decapStep * deltaTime);
                    }

                }
            }
        }

        private (int playerId, float maxValue) GetMaxPlayerControlValue(int2 hexCoord)
        {
            if (!_hexIndices.TryGetValue(hexCoord, out var hexDataReadIndex))
            {
                return (-1, 0f);
            }

            var playerId = -1;
            var maxValue = 0f;

            for (var i = 0; i < _playersCount; i++)
            {
                var controlValue = _controlValues[GetPlayerControlValueIndexByIndex(i, hexDataReadIndex)];

                if (controlValue > maxValue)
                {
                    maxValue = controlValue;
                    playerId = i;
                }
            }

            return (playerId, maxValue);
        }

        private void MarkHexAsClear(ref HexCaptureStateComponent component)
        {
            component.CaptureProgress = 0f;
            component.IsCaptured = false;
            component.ControllingPlayerId = -1;
        }

        private void DoCaptureProgress(ref HexCaptureStateComponent component, int playerId, float step)
        {
            component.ControllingPlayerId = playerId;
            component.CaptureProgress = MathExtensions.MoveTowards(component.CaptureProgress, 1f, step);            
            component.IsCaptured = component.CaptureProgress == 1f;
        }

        private void DoDecap(ref HexCaptureStateComponent component, float step)
        {
            component.CaptureProgress = MathExtensions.MoveTowards(component.CaptureProgress, 0f, step);
            component.IsCaptured = false;            
            if (component.CaptureProgress == 0f)
                MarkHexAsClear(ref component);
        }
    }
}