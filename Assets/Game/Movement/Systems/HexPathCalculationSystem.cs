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
        private readonly HexPathsLRUBuffer _hexPaths;
        private readonly RequestedHexPathsList _requestedHexPathsList;
        private readonly HexPathCalculationProcessManager _processesManager;
        private readonly HexPathKey[] _calculationRequests;
        private const int MAX_PARALLEL_CALCULATIONS = 1;


        [Inject]
        public HexPathCalculationSystem(HexPathsLRUBuffer hexPaths, RequestedHexPathsList requestedHexPathsList, INavigationMap map)
        {
            _hexPaths = hexPaths;
            _requestedHexPathsList = requestedHexPathsList;

            _map = map;
            _processesManager = new(Allocator.Persistent, _map, MAX_PARALLEL_CALCULATIONS, hexPaths);
            _calculationRequests = new HexPathKey[MAX_PARALLEL_CALCULATIONS];
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            if (!_map.IsInitialized)
                return;


            var idleProcessesCount = _processesManager.UpdateAndGetIdleProcessesCount();
            var scheduledCount = 0;
            foreach (var request in _requestedHexPathsList)
            {
                var token = _processesManager.TryLaunchProcess(request.Start, request.End);
                if (!token.IsValid)
                    break;

                idleProcessesCount--;
                _calculationRequests[scheduledCount++] = request;

                if (idleProcessesCount == 0)
                    break;
            }

            if (scheduledCount != 0)
            {
                for (var i = 0; i < scheduledCount; i++)
                {
                    var pathKey = _calculationRequests[i];
                    _requestedHexPathsList.Remove(pathKey);
                    //UnityEngine.Debug.Log($"scheduled {pathKey.Start}->{pathKey.End}");
                }
            }
        }

        public void Dispose()
        {
            _processesManager.Dispose();
        }
    }
}