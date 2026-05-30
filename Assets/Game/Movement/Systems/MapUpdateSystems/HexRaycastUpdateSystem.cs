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
        private const int MAX_PROCESSES_COUNT = 4;

        [Inject]
        public HexRaycastUpdateSystem(HexRaycastRequestsList requestsList, IUpdatableMap map)
        {
            _requestsList = requestsList;
            _processesManager = new(Allocator.Persistent, map, MAX_PROCESSES_COUNT);
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
            var requestsCount = _requestsList.Count;
            var processesCount = _calculatingProcesses.Count;
            var pool = ArrayPool<int2>.Shared;
            var clearList = pool.Rent(math.max(requestsCount, processesCount));
            var clearCount = 0;

            foreach (var calculatingProcessKvp in _calculatingProcesses)
            {
                if (_processesManager.IsProcessCompleted(calculatingProcessKvp.Value))
                    clearList[clearCount++] = calculatingProcessKvp.Key;
            }
            if (clearCount != 0)
            {
                for (var i = 0; i < clearCount; i++)
                {
                    _calculatingProcesses.Remove(clearList[i]);
                }
            }
            pool.Return(clearList);
        }

        private void HandleRequests()
        {
            var idleProcesses = _processesManager.UpdateAndGetIdleProcessesCount();
            foreach (var request in _requestsList)
            {
                var hexCoord = request.HexCoord;
                var hexVersion = request.HexVersion;

                if (_calculatingProcesses.TryGetValue(hexCoord, out var token))
                {
                    // if process trying to calculate outdated version - stop it
                    if (token.HexVersion < hexVersion)
                        _processesManager.StopProcess(token.ProcessIndex);
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
                }
            }
        }
    }
}