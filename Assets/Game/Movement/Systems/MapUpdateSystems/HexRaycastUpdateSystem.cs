using System.Buffers;
using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexRaycastUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private readonly HexRaycastRequestsList _requestsList;
        private readonly Dictionary<int2,HexRaycastProcessToken> _calculatingProcesses = new();
        private readonly HexRaycastProcessesManager _processesManager;
        private readonly UpdateEdgeExitsRequestsList _portalCalculationRequests;
        private readonly IUpdatableMap _map;
        private readonly ArrayPool<int2> _pool;
        private const int MAX_PROCESSES_COUNT = 4;
        

        [Inject]
        public HexRaycastUpdateSystem(HexRaycastRequestsList requestsList, IUpdatableMap map, UpdateEdgeExitsRequestsList portalRequestsList)
        {
            _requestsList = requestsList;
            _map = map;
            _processesManager = new(Allocator.Persistent, _map, MAX_PROCESSES_COUNT);      
            _pool = ArrayPool<int2>.Shared;
            _portalCalculationRequests = portalRequestsList;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            CheckActiveProcesses();            
            HandleRequests();
        }

        public void Dispose()
        {
            _processesManager.Dispose();
        }

        private void CheckActiveProcesses()
        {
            var processesCount = _calculatingProcesses.Count;
            var clearList = _pool.Rent(processesCount);
            var clearCount = 0;

            foreach (var calculatingProcessKvp in _calculatingProcesses)
            {
                var token = calculatingProcessKvp.Value;
                if (_processesManager.IsProcessCompleted(token))
                {
                    var hexCoord = calculatingProcessKvp.Key;
                    clearList[clearCount++] = hexCoord;
                    _portalCalculationRequests.Add(hexCoord);
                }                    
            }

            if (clearCount != 0)
            {
                for (var i = 0; i < clearCount; i++)
                {
                    _calculatingProcesses.Remove(clearList[i]);                    
                }
                _map.UpdateVersion();
                //UnityEngine.Debug.Log($"map update to version {_map.Version}");
            }

            _pool.Return(clearList);
        }

        private void HandleRequests()
        {
            var idleProcesses = _processesManager.UpdateAndGetIdleProcessesCount();   
            
            var requestsCount = _requestsList.Count;
            if (requestsCount == 0)
                return;

            var clearList = _pool.Rent(requestsCount);
            var clearCount = 0;

            foreach (var request in _requestsList)
            {
                var hexCoord = request.HexCoord;
                var requestHexVersion = request.HexPassabilityVersion;
                var currentHexVersion = _map.GetOrCreateHex(hexCoord).PassabilityVersion;

                if (currentHexVersion > requestHexVersion)
                {
                    clearList[clearCount] = hexCoord;
                    continue;
                }

                if (_calculatingProcesses.TryGetValue(hexCoord, out var token))
                {
                    // if process trying to calculate outdated version - stop it
                    if (token.HexVersion < requestHexVersion)
                    {
                        _processesManager.StopProcess(token.ProcessIndex);
                    }      
                    else
                    {
                        clearList[clearCount++] = hexCoord;
                    }
                }
                else
                {
                    if (idleProcesses == 0)
                        continue;

                    token = _processesManager.TryLaunchProcess(request);
                    if (!token.IsValid)
                    {
                        idleProcesses = 0;
                    }
                    else
                    {
                        // can overwrite old outdated process
                        _calculatingProcesses[hexCoord] = token;
                        idleProcesses--;
                    }
                    clearList[clearCount++] = hexCoord;
                }
            }

            if (clearCount != 0)
            {
                for (var i = 0; i < clearCount; i++)
                {
                    _requestsList.RemoveActualRequest(clearList[i]);
                }
            }
            _pool.Return(clearList);
        }
    }
}