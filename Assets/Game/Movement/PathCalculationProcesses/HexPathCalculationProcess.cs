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

        public HexPathCalculationProcess(Allocator allocator, INavigationMap map)
        {
            _collections = PrepareHexPathJobCollectionsCommand.Execute(allocator, map);
            _job = new()
            {
                PathCost = _collections.PathCost,
                HexData = _collections.HexData,
                NavigationData = _collections.NavigationData,
                ResultingData = _collections.ResultingData,
                OpenedList = _collections.OpenedList,
            };
        }

        protected override JobHandle LaunchJob(PathInput<HexPathNodeKey> input)
        {
            _job.Start = input.Start;
            _job.End = input.End;
            return _job.ScheduleByRef();
        }

        protected override PathCalculationResult<HexPathNodeKey> FormResults()
        {
            var rawResultsData = _collections.ResultingData;
            var resultsLength = rawResultsData.Length;
            var lastNode = resultsLength == 0 ? default : rawResultsData[resultsLength - 1];
            var hasReachedTarget = lastNode == _job.End;

            return new PathCalculationResult<HexPathNodeKey>(
                requestedDestination: (_job.Start, _job.End),
                points: HexUpdateLogic.RefineHexPath(_job.Start.HexCoord, rawResultsData), 
                pathCost: _collections.PathCost.Value, 
                hasReachedTarget: hasReachedTarget);
        }
            

        protected override void DisposeResources()
        {
            _job = default;
            _collections.Dispose();
        }
    }
}
