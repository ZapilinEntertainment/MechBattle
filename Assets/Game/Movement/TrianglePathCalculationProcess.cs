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

namespace ZE.MechBattle.Ecs
{
    public class TrianglePathCalculationProcess : IDisposable
    {
        public CalculationProcessStage Stage => _isLaunched
            ? (_activeHandle.IsCompleted ? CalculationProcessStage.Complete : CalculationProcessStage.Calculating)
            : CalculationProcessStage.Idle;


        public int PathId { get; private set; }
        public int ProcessIteration { get; private set; }

        private readonly TriangularPathJobCollections _collections;
        private readonly INavigationMap _map;

        private ConstructTriangularPathJob _job;
        private JobHandle _activeHandle;
        private bool _isLaunched;

        public TrianglePathCalculationProcess(Allocator allocator, INavigationMap map)
        {
            _map = map;
            _collections = new(allocator, default, _map.Settings);
            _job = new()
            {
                CalculationData = _collections.CalculationData,
                OpenedList = _collections.OpenedList,
                PassabilityData = _collections.PassabilityData,
                ResultList = _collections.ResultList
            };
        }

        public void Launch(int pathId, IntTriangularPos start, IntTriangularPos end)
        {
            PathId = pathId;
            ChangeTrianglePathJobSetupDataCommand.Execute(ref _job, _collections, start, _map);

            _job.Start = start;
            _job.End = end;
            
            _activeHandle = _job.ScheduleByRef();
            _isLaunched = true;
            ProcessIteration++;

            //UnityEngine.Debug.Log($"launched triangle path calculation: {start} -> {end}, {_collections.PassabilityData.TryGetIndex(start, out _)} {_collections.PassabilityData.TryGetIndex(end, out _)}");
        }

        public NativeArray<IntTriangularPos> Stop()
        {
            _activeHandle.Complete();
            _isLaunched = false;
            ProcessIteration++;
            return _collections.ResultList.AsArray();
        }

        public async void Dispose()
        {
            if (_isLaunched)
            {
                while (!_activeHandle.IsCompleted)
                    await Task.Delay(100);
            }
            _job = default;
            _activeHandle = default;
            _collections.Dispose();
        }
    }
}
