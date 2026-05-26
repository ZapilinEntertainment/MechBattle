using System;
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
        private DefineCellZoneJob _job;

        public DefineCellZoneProcess(Allocator allocator, IUpdatableMap map)
        {
            var trianglesPerHex = map.TrianglesPerHex;
            _hexRadius = map.TrianglesPerHexEdge;

            _activeCells = new(allocator);
            _cells = new(trianglesPerHex, allocator);

            _job = new DefineCellZoneJob()
            {
                ActiveCells = _activeCells,
                Cells = _cells,
            };
        }

        public void Dispose()
        {
            _activeCells.Dispose();
            _cells.Dispose();
        }

        public JobHandle ScheduleJob(IntTriangularPos hexCenter, IPassabilityDataSource passabilityDataSource)
        {
            _job.HexCenter = hexCenter;
            _activeCells.Clear();
            _cells.Clear();

            foreach (var pos in new HexTrianglesEnumerator(hexCenter, _hexRadius))
            {
                var passabilityData = passabilityDataSource.GetPassabilityData(pos);
                _cells.Add(pos, new(passabilityData, isOpened: false, zoneIndex: 0));
            }

            return _job.ScheduleByRef();
        }

        public int GetZoneIndex(IntTriangularPos pos) => _cells[pos].ZoneIndex;
    }
}
