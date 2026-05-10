using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class HexPathCalculationProcess : PathCalculationProcess<HexPathNodeKey>
    {
        private readonly HexPathJobCollections _collections;
        private ConstructHexPathJob _job;

        public HexPathCalculationProcess(Allocator allocator, INavigationMap map) : base(map)
        {
            _collections = new(allocator, map.Hexes.Count);
            _job = new()
            {
                PathCost = _collections.PathCost,
                HexData = _collections.HexData,
                NavigationData = _collections.NavigationData,
                ResultingData = _collections.ResultingData,
                OpenedList = _collections.OpenedList,
            };
        }

        protected override JobHandle LaunchJob(HexPathNodeKey start, HexPathNodeKey end)
        {
            _job.Start = start;
            _job.End = end;
            return _job.ScheduleByRef();
        }

        protected override CalculatedPathData<HexPathNodeKey> GetJobResults() =>
            new(_collections.ResultingData.AsArray(), _collections.PathCost.Value);

        protected override void DisposeResources()
        {
            _job = default;
            _collections.Dispose();
        }
    }
}
