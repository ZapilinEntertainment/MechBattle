using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class TrianglePathCalculationProcess : PathCalculationProcess<IntTriangularPos>
    {
        private readonly INavigationMap _map;
        private readonly TriangularPathJobCollections _collections;
        private ConstructTriangularPathJob _job;

        public TrianglePathCalculationProcess(Allocator allocator, INavigationMap map)
        {
            _map = map;
            _collections = new(allocator, default, map.Settings);
            _job = new()
            {
                CalculationData = _collections.CalculationData,
                OpenedList = _collections.OpenedList,
                PassabilityData = _collections.PassabilityData,
                ResultList = _collections.ResultList,
                PathCost = _collections.PathCostReference
            };
        }

        protected override JobHandle LaunchJob(PathInput<IntTriangularPos> input)
        {
            ChangeTrianglePathJobSetupDataCommand.Execute(ref _job, _collections, input.Start, _map);
            _job.Start = input.Start;
            _job.End = input.End;
            return _job.ScheduleByRef();
        }

        protected override PathCalculationResult<IntTriangularPos> FormResults()
        {
            var rawResultsData = _collections.ResultList;
            var resultsLength = rawResultsData.Length;
            var lastNode = resultsLength == 0 ? default : rawResultsData[resultsLength - 1];
            var hasReachedTarget = lastNode == _job.End;
            return new PathCalculationResult<IntTriangularPos>(
                requestedDestination: (_job.Start, _job.End), 
                points: rawResultsData.AsArray(), 
                pathCost: _collections.PathCostReference.Value, 
                hasReachedTarget: hasReachedTarget);
        }

        protected override void DisposeResources()
        {
            _job = default;
            _collections.Dispose();
        }
    }
}
