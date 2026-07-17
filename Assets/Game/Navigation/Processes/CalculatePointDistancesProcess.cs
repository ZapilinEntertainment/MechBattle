using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle
{
    public readonly struct CalculatePointDistancesResults
    {
        public readonly int PortalId;
        public readonly int2 HexCoord;
        private readonly NativeHashMap<IntTriangularPos, CalculatePointDistancesJob.CellData>.ReadOnly _originalData;

        public CalculatePointDistancesResults(
            NativeHashMap<IntTriangularPos, CalculatePointDistancesJob.CellData>.ReadOnly originalData, 
            int2 hexCoord,
            int portalId)
        {
            _originalData = originalData;
            HexCoord = hexCoord;
            PortalId = portalId;
        }

        public float GetDistance(IntTriangularPos pos) => _originalData[pos].Distance;
        public bool TryGetDistance(IntTriangularPos pos, out float distance)
        {
            if (_originalData.TryGetValue(pos, out var cellData))
            {
                distance = cellData.Distance;
                return distance != CalculatePointDistancesProcess.DEFAULT_DISTANCE;
            }

            distance = float.MaxValue;
            return false;
        }
    }

    public class CalculatePointDistancesProcess : JobProcessBase<CalculatePointDistancesLaunchData, CalculatePointDistancesResults>
    {
        public const float DEFAULT_DISTANCE = float.MaxValue;

        private readonly INavigationMap _map;
        private readonly NativeHashMap<IntTriangularPos, CalculatePointDistancesJob.CellData> _cells;
        private readonly NativeQueue<IntTriangularPos> _activeCells;
        private CalculatePointDistancesJob _job;
        private int2 _hexCoord;
        private int _portalId;
        private JobHandle _activeJobHandle;

        public CalculatePointDistancesProcess(Allocator allocator, INavigationMap map)
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

        public CalculatePointDistancesResults Run(CalculatePointDistancesLaunchData input)
        {
            PrepareJob(input);
            _job.Run();
            _activeJobHandle = default;

            return FormResults();
        }

        protected override CalculatePointDistancesResults FormResults() => new(_job.Cells.AsReadOnly(), _hexCoord, _portalId);

        protected override JobHandle LaunchJob(CalculatePointDistancesLaunchData input)
        {
            PrepareJob(input);
            _activeJobHandle = _job.Schedule();
            return _activeJobHandle;
        }

        protected override void DisposeResources()
        {
#if UNITY_EDITOR
            if (!_activeJobHandle.IsCompleted)
                UnityEngine.Debug.LogWarning($"{nameof(CalculatePointDistancesProcess)} is not yet completed");

            try
            {
                FinalDispose();
            }
            catch (Exception ex)
            {
                if (!ZE.Utils.EditorPlaymodeLifetimeObject.IsQuitting)
                    UnityEngine.Debug.LogError(ex);
            }
            return;
#else  

            FinalDispose();       
#endif            
        }

        private void FinalDispose() 
        {
            _activeJobHandle.Complete();
            _activeCells.Dispose();
            _cells.Dispose();            
        }

        private void PrepareJob(CalculatePointDistancesLaunchData input)
        {
            _hexCoord = input.HexCoord;
            _portalId = input.PortalId;
            _job.ZeroPos = input.CenterPos;

            _cells.Clear();
            _activeCells.Clear();
            var hexPos = new NavigationHexPosition(_hexCoord, _map);
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, _map.TrianglesPerHexEdge))
            {
                _cells.Add(tripos, new() { Distance = DEFAULT_DISTANCE, IsOpened = false, Passability = _map.GetPassabilityData(tripos) });
            }

            if (!_cells.ContainsKey(_job.ZeroPos))
            {
                UnityEngine.Debug.LogError($"{_job.ZeroPos} is not in {_hexCoord} (portal {_portalId})");
            }                
        }
    }
}
