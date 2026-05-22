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
        private enum CalculationStatus : byte
        {
            Idle, Calculating, Completed
        }

        private struct CalculationProcess
        {
            public CalculationStatus Status;
            public HexPathSearchRequest Request;
        }

        public World World { get; set;}

        private readonly INavigationMap _map;
        private readonly RequestedHexPathsList _requestedHexPathsList;
        private readonly HexPathsSearchHistory _searchHistory;
        private readonly HexPathsLRUBuffer _hexPaths;
        private readonly List<HexPathSearchRequest> _clearList;

        private CalculationProcess _calculationProcess;

        [Inject]
        public HexPathCalculationSystem(
            RequestedHexPathsList requestedHexPathsList, 
            HexPathsSearchHistory searchHistory,
            HexPathsLRUBuffer hexPaths,
            INavigationMap map)
        {
            _requestedHexPathsList = requestedHexPathsList;
            _map = map;
            _searchHistory = searchHistory;
            _hexPaths = hexPaths;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            if (!_map.IsInitialized)
                return;

            CheckCalculatingProcess();
            HandleReceivedRequests();
        }

        public void Dispose() { }

        private void CheckCalculatingProcess()
        {
            if (_calculationProcess.Status == CalculationStatus.Completed)
            {
                _requestedHexPathsList.Remove(_calculationProcess.Request);
                _hexPaths.AddCalculatedPath()
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