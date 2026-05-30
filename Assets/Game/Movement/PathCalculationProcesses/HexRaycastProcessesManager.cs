using System;
using Unity.Mathematics;
using Unity.Collections;
using ZE.Utils;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct HexRaycastProcessToken : IProcessToken
    {
        public bool IsValid => _isValid;

        public int ProcessIteration => _processIteration;

        public int ProcessIndex => _processIndex;

        private readonly bool _isValid;
        private readonly int _processIndex;
        private readonly int _processIteration;
        public readonly int2 HexCoord;
        public readonly int HexVersion;

        public HexRaycastProcessToken(int processIndex, int processIteration, HexUpdateRequest request)
        {
            _isValid = true;
            _processIndex = processIndex;
            _processIteration = processIteration;
            HexCoord = request.HexCoord;
            HexVersion = request.HexVersion;
        }
    }

    public class HexRaycastProcessesManager : ProcessManagerBase<HexRaycastProcess, HexUpdateRequest, HexRaycastProcessToken>
    {
        private readonly Allocator _allocator;
        private readonly IUpdatableMap _map;

        public HexRaycastProcessesManager(Allocator allocator, IUpdatableMap map, int maxProcessesCount) : base(maxProcessesCount)
        {
            _allocator = allocator;
            _map = map;
        }

        public void StopProcess(int processIndex) => Processes[processIndex].Stop();

        protected override HexRaycastProcess CreateNewProcess() => new(_allocator, _map);

        protected override HexRaycastProcessToken LaunchProcess(HexUpdateRequest request, HexRaycastProcess process, int processIndex)
        {
            var token = new HexRaycastProcessToken(processIndex, process.ProcessIteration, request);
            process.LaunchAsync(request.HexCoord); // forget, but managers watches its status
            return token;
        }

        protected override void HandleResults(HexRaycastProcess process)
        {
            if (process.WasStopped)
                return;

            process.ApplyCalculatedData(process.CurrentHexPosition.TriangularCenterPos);
        }        
    }
}
