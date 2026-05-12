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

        protected override JobHandle LaunchJob(IntTriangularPos start, IntTriangularPos end)
        {
            ChangeTrianglePathJobSetupDataCommand.Execute(ref _job, _collections, start, _map);
            _job.Start = start;
            _job.End = end;
            return _job.ScheduleByRef();
        }

        protected override CalculatedPathData<IntTriangularPos> GetJobResults() =>
            new(_collections.ResultList.AsArray(), _collections.PathCostReference.Value);

        protected override void DisposeResources()
        {
            _job = default;
            _collections.Dispose();
        }
    }
}
