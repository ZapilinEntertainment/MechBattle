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
        private readonly struct HexDataKey
        {
            public readonly int PlayerId;
            public readonly int2 HexCoord;

            public HexDataKey(int2 hexCoord, int playerId)
            {
                PlayerId = playerId;
                HexCoord = hexCoord;
            }
        }

        private struct PlayerCapturePowers
        {
            public float ActiveCapturePower;
            public float ResistPower;

            public float TotalPower => ActiveCapturePower + ResistPower;
        }

        private Filter _captureUnitsFilter;
        private Filter _hexEntitiesFilter;

        private Stash<HexCoordComponent> _hexCoords;
        private Stash<PlayerAffiliationComponent> _affiliations;
        private Stash<CaptureBehaviourComponent> _capturePowerComponents;

        private Stash<HexCaptureStateComponent> _hexCaptureState;
        private Stash<HexComponent> _hexComponents;
        private Stash<HexOwnerChangedTag> _ownerChangedTag;

        private int _playersCount = 1;
        private readonly IPlayersList _playersList;
        private readonly Dictionary<HexDataKey, PlayerCapturePowers> _powersData = new(16);



        [Inject]
        public HexCaptureStatusCalculationSystem(SceneFlagsManager flags, IPlayersList playersList) : base(flags)
        {
            _playersList = playersList;
        }

        public override void OnAwake()
        {
            _captureUnitsFilter = World.Filter
                .With<CaptureBehaviourComponent>()
                .With<PlayerAffiliationComponent>()
                .With<HexCoordComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _hexCoords = World.GetStash<HexCoordComponent>();
            _affiliations = World.GetStash<PlayerAffiliationComponent>();
            _capturePowerComponents = World.GetStash<CaptureBehaviourComponent>();
            _ownerChangedTag = World.GetStash<HexOwnerChangedTag>();



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

            _powersData.Clear();
        }

        private void CalculatePlayerCaptureValues()
        {
            _playersCount = _playersList.Count;

            foreach (var unit in _captureUnitsFilter)
            {
                var captureBehaviour = _capturePowerComponents.Get(unit).Value;
                switch(captureBehaviour.Type)
                {
                    case CaptureBehaviourType.CanCapture:
                        {
                            AddActiveCapturePower(unit, captureBehaviour.Value);
                            break;
                        }
                    case CaptureBehaviourType.ResistCapturing:
                        {
                            AddResistCapturePower(unit, captureBehaviour.Value);
                            break;
                        }
                    default: continue;
                }
            }
        }

        private (HexDataKey Key, PlayerCapturePowers Data) GetCaptureContext(Entity unit)
        {
            var hexCoord = _hexCoords.Get(unit).Value;
            var playerKey = _affiliations.Get(unit).PlayerKey;
            var dataKey = new HexDataKey(hexCoord, playerKey.Id);

            _powersData.TryGetValue(dataKey, out var capData);
            return (dataKey, capData);
        }

        private void AddActiveCapturePower(Entity unit, float value)
        {
            var (key, data) = GetCaptureContext(unit);
            data.ActiveCapturePower += value;
            _powersData[key] = data;
        }

        private void AddResistCapturePower(Entity unit, float value)
        {
            var (key, data) = GetCaptureContext(unit);
            data.ResistPower += value;
            _powersData[key] = data;
        }



        private void UpdateHexCaptureStatuses(float deltaTime)
        {
            foreach (var hexEntity in _hexEntitiesFilter)
            {
                var hexCoord = _hexComponents.Get(hexEntity).Coord;
                var (maxPowerfulPlayerId, maxPowerfulPlayerPowers) = GetPlayerWithMaxActiveCapturePowers(hexCoord);

                ref var hexCaptureStateComponent = ref _hexCaptureState.Get(hexEntity);
                var currentOwnerPlayerId = hexCaptureStateComponent.ControllingPlayerId;
                PlayerCapturePowers existingPlayerPowers = default;
                var currentOwnerExists = currentOwnerPlayerId != -1 && _powersData.TryGetValue(new(hexCoord, currentOwnerPlayerId), out existingPlayerPowers);
                var capturerExists = maxPowerfulPlayerId != -1;
                var ownerChanged = false;

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
                        ownerChanged = DoDecap(ref hexCaptureStateComponent, CaptureConstants.EMPTY_HEX_DECAP_SPEED * deltaTime, 0f);
                    }
                }
                else
                {
                    var powersSum = existingPlayerPowers.TotalPower + maxPowerfulPlayerPowers.TotalPower;
                    var alreadyOwns = hexCaptureStateComponent.ControllingPlayerId == maxPowerfulPlayerId;
                    if (!currentOwnerExists || alreadyOwns)
                    {
                        if (alreadyOwns && hexCaptureStateComponent.IsCaptured)
                            return;

                        // if empty hex or player is already owns it
                        var captureStep = math.clamp(maxPowerfulPlayerPowers.ActiveCapturePower * CaptureConstants.CAPTURE_SPEED_CF, 0f, CaptureConstants.MAX_CAPTURE_SPEED);
                        ownerChanged = DoCaptureProgress(ref hexCaptureStateComponent, maxPowerfulPlayerId, captureStep * deltaTime, 1f - existingPlayerPowers.ResistPower / powersSum);
                    }
                    else
                    {
                        // taking controls from another player

                        var decapStep = (maxPowerfulPlayerPowers.ActiveCapturePower - existingPlayerPowers.ActiveCapturePower) * CaptureConstants.FIGHT_CAPTURE_SPEED_CF;
                        decapStep = math.clamp(decapStep, 0f, CaptureConstants.MAX_DECAP_SPEED);
                        
                        ownerChanged = DoDecap(ref hexCaptureStateComponent, decapStep * deltaTime, existingPlayerPowers.ResistPower / powersSum);
                    }
                }

                if (ownerChanged)
                    _ownerChangedTag.Set(hexEntity);
            }
        }

        private (int playerId, PlayerCapturePowers capturePowers) GetPlayerWithMaxActiveCapturePowers(int2 hexCoord)
        {
            var playerId = -1;
            PlayerCapturePowers capturePowers = default;
            for (var i = 0; i < _playersCount; i++)
            {
                if (!_powersData.TryGetValue(new(hexCoord, i), out var playerPowers))
                    continue;

                if (playerPowers.ActiveCapturePower > capturePowers.ActiveCapturePower)
                {
                    playerId = i;
                    capturePowers = playerPowers;
                }
            }

            return (playerId, capturePowers);
        }

        private void MarkHexAsClear(ref HexCaptureStateComponent component)
        {
            component.CaptureProgress = 0f;
            component.IsCaptured = false;
            component.ControllingPlayerId = -1;
        }

        private bool DoCaptureProgress(ref HexCaptureStateComponent component, int playerId, float step, float limit)
        {
            component.ControllingPlayerId = playerId;
            component.CaptureProgress = MathExtensions.MoveTowards(component.CaptureProgress, limit, step);            
            component.IsCaptured = component.CaptureProgress == 1f;
            return component.IsCaptured;
        }

        private bool DoDecap(ref HexCaptureStateComponent component, float step, float limit)
        {
            component.CaptureProgress = MathExtensions.MoveTowards(component.CaptureProgress, limit, step);
            component.IsCaptured = false;            
            if (component.CaptureProgress == 0f)
            {
                MarkHexAsClear(ref component);
                return true;
            }

            return false;
                
        }
    }
}