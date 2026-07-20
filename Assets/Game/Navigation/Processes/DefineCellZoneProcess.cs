using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class DefineCellZoneProcess : IDisposable
    {
        private readonly int _hexRadius;
        private readonly NativeQueue<IntTriangularPos> _activeCells;
        private readonly NativeHashMap<IntTriangularPos, DefineCellZoneJob.CellData> _cells;
        private NativeArray<IntTriangularPos> _hexTris;
        private DefineCellZoneJob _job;
        private JobHandle _activeHandle;

        public DefineCellZoneProcess(Allocator allocator, IUpdatableMap map)
        {
            var trianglesPerHex = map.HexTrianglesCount;
            _hexRadius = map.TrianglesPerHexEdge;

            _activeCells = new(allocator);
            _cells = new(trianglesPerHex, allocator);
            _hexTris = new(trianglesPerHex, allocator, NativeArrayOptions.UninitializedMemory);

            _job = new DefineCellZoneJob()
            {
                ActiveCells = _activeCells,
                Cells = _cells,
                HexTris = _hexTris,
            };
        }

        public void Dispose()
        {
            _activeHandle.Complete();
            _activeCells.Dispose();
            _cells.Dispose();
        }

        public JobHandle ScheduleJob(IntTriangularPos hexCenter, CellPassabilityData[] hexCellPassabilityData)
        {
            _activeCells.Clear();
            _cells.Clear();

            var i =0;
            foreach (var pos in new HexTrianglesEnumerator(hexCenter, _hexRadius))
            {
                _hexTris[i] = pos;
                var passabilityData = hexCellPassabilityData[i];
                _cells.Add(pos, new(passabilityData, isOpened: false, zoneIndex: 0));

                i++;
            }

            _activeHandle =  _job.ScheduleByRef();
            return _activeHandle;
        }

        public void GetResults(int[] receiverArray)
        {
            _activeHandle.Complete();
            for (var i = 0; i < _hexTris.Length; i++)
            {
                receiverArray[i] = _cells[ _hexTris[i]].ZoneIndex;
            }
        }
    }
}
