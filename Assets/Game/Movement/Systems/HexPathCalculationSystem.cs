using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathCalculationSystem : ISystem 
    {

        public World World { get; set;}

        private readonly INavigationMap _map;
        private readonly RequestedHexPathsList _requestedHexPathsList;
        private readonly PortalsPathCalculationProcessesManager _processesManager;
        private readonly AwaitingTokensList _awaitingTokensList;
        private readonly HexDataAccessHandler _hexDataAccessHandler;
        private readonly List<HexPathSearchRequest> _clearList;
        private readonly Dictionary<HexPathSearchRequest, PathCalculationProcessToken> _calculatingTokens = new();


        [Inject]
        public HexPathCalculationSystem(
            RequestedHexPathsList requestedHexPathsList, 
            INavigationMap map, 
            AwaitingTokensList awaitingTokensList,
            HexDataAccessHandler hexDataAccessHandler)
        {
            _requestedHexPathsList = requestedHexPathsList;
            _awaitingTokensList = awaitingTokensList;
            _hexDataAccessHandler = hexDataAccessHandler;

            _map = map;
            _processesManager = new(MAX_PARALLEL_CALCULATIONS, _hexPaths);
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            if (!_map.IsInitialized)
                return;

            CheckCalculatingProcesses();
            var idleProcessesCount = _processesManager.UpdateAndGetIdleProcessesCount();
            if (idleProcessesCount == 0)
                return;

            HandleReceivedRequests(idleProcessesCount);
        }

        public void Dispose()
        {
            _processesManager.Dispose();
        }

        private void CheckCalculatingProcesses()
        {
            foreach (var kvp in _calculatingTokens)
            {
                if (_processesManager.IsProcessCompleted(kvp.Value))
                {
                    _clearList.Add(kvp.Key);
                    _requestedHexPathsList.Remove(kvp.Key);
                }
            }

            var count = _clearList.Count;
            if (count != 0)
            {
                for (var i = 0; i < count; i++)
                {
                    _calculatingTokens.Remove(_clearList[i]);
                }
                _clearList.Clear();
            }
        }

        private void HandleReceivedRequests(int idleProcessesCount)
        {
            foreach (var request in _requestedHexPathsList)
            {
                if (_calculatingTokens.ContainsKey(request))
                    continue;

                var calculationToken = _processesManager
                    .TryLaunchProcess(
                    new(request.StartHexCoord, request.StartHexZoneIndex),
                    new(request.EndHexCoord, request.EndHexZoneIndex));
                if (!calculationToken.IsValid)
                    return;

                _calculatingTokens.Add(request, calculationToken);

                idleProcessesCount--;
                _clearList.Add(request);

                if (idleProcessesCount == 0)
                    break;
            }
        }
    }
}