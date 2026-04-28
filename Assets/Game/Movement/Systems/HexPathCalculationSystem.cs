using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private readonly NavigationHexPathsList _pathsList;
        private readonly INavigationMap _map;  

        private bool _isTrackingActiveHandle = false;
        private int _currentMapVersion;
        private JobHandle _activeHandle;
        private HexPathKey _calculatingPathKey;
        private HexPathJobCollections _jobDataCollection;

        [Inject]
        public HexPathCalculationSystem(NavigationHexPathsList list, INavigationMap map)
        {
            _pathsList = list;
            _map = map;

            var capacity = map.Hexes.Count;
        }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {            
            if (!_map.IsInitialized)
                return;

            if (_isTrackingActiveHandle)
            {
                if (!_activeHandle.IsCompleted)
                    return;

                _activeHandle.Complete();
                _isTrackingActiveHandle = false;

                var points = _jobDataCollection.ResultingData.AsArray().ToArray();
                var path = new HexPath(points, _jobDataCollection.PathCost.Value);
                _pathsList.AddCalculatedPath(path);

            }

            if (!_pathsList.TryGetRequestedPath(out var pathKey))
                return;

            if (_map.Version != _currentMapVersion)
            {
                _jobDataCollection?.Dispose();
                _jobDataCollection = PrepareHexPathJobCollectionsCommand.Execute(Allocator.Persistent, _map);
                _currentMapVersion = _map.Version;
            }

            var job = new ConstructHexPathJob()
            {
                HexData = _jobDataCollection.HexData,
                NavigationData = _jobDataCollection.NavigationData,
                ResultingData = _jobDataCollection.ResultingData,
                OpenedList = _jobDataCollection.OpenedList,
                PathCost = _jobDataCollection.PathCost,

                Start = pathKey.Start,
                End = pathKey.End,
            };
            _activeHandle = job.ScheduleByRef();
            _calculatingPathKey = pathKey;
            _isTrackingActiveHandle = true;
        }

        public void Dispose()
        {
            _jobDataCollection?.Dispose();
        }
    }
}