using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class GeneratePointDistancesProcess : IDisposable
    {
        private readonly INavigationMap _map;
        private readonly NativeHashMap<IntTriangularPos, GeneratePointDistancesJob.CellData> _cells;
        private readonly NativeQueue<IntTriangularPos> _activeCells;

        private bool _isJobExecuting = false;
        private JobHandle _activeJobHandle;
        private GeneratePointDistancesJob _job;
    

        public GeneratePointDistancesProcess(Allocator allocator, INavigationMap map)
        {
            _map = map;

            var trianglesInHexCount = TriangularMath.GetTrianglesCountInHex(map.TrianglesPerHexEdge);
            _cells = new(trianglesInHexCount, allocator);
            _activeCells = new(allocator);

            _job = new()
            {
                ActiveCells = _activeCells,
                Cells= _cells                
            };
        }

        public async void Dispose()
        {
            if (_isJobExecuting)
            {
                while (!_activeJobHandle.IsCompleted)
                    await Task.Delay(100);
            }

            _activeCells.Dispose();
            _cells.Dispose();
        }

        public JobHandle Schedule(int2 hexCoord, IntTriangularPos zeroPos)
        {
            if (_isJobExecuting)
                throw new Exception("process is in use");

            _job.ZeroPos = zeroPos;

            _cells.Clear();
            _activeCells.Clear();
            var hexPos = new NavigationHexPosition(hexCoord, _map);
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, _map.TrianglesPerHexEdge))
            {
                _cells.Add(tripos, new() { Distance = float.MaxValue, IsOpened = false, Passability = _map.GetPassabilityData(tripos)});
            }

            _activeJobHandle = _job.Schedule();
            _isJobExecuting = true;
            return _activeJobHandle;
        }

        public void UnloadDistanceDataInto(Dictionary<IntTriangularPos, float> distances)
        {
            distances.Clear();
            foreach (var kvp in _cells)
            {
                distances.Add(kvp.Key, kvp.Value.Distance);
            }
        }
    }
}
