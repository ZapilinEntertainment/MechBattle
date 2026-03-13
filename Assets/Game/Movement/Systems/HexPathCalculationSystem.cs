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
        private readonly NavigationPathsList _pathsList;
        private readonly NavigationMap _map;  
        private readonly NativeHashMap<int2, NavigationNodeData> _nodeData;
        private readonly NativeHashSet<int2> _openedList;
        private readonly NativeList<int2> _resultingList;

        private bool _isTrackingActiveHandle = false;
        private JobHandle _activeHandle;
        private int4 _calculatingPathStartEnd;

        [Inject]
        public HexPathCalculationSystem(NavigationPathsList list, NavigationMap map)
        {
            _pathsList = list;
            _map = map;

            var capacity = map.Hexes.Count;
            _nodeData = new(capacity, Allocator.Persistent);
            _openedList = new(capacity / 2, Allocator.Persistent);
            _resultingList = new(capacity / 2, Allocator.Persistent);
        }

        // todo: update hexNodesData on start and on changes

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            if (_isTrackingActiveHandle)
            {
                if (!_activeHandle.IsCompleted)
                    return;

                _activeHandle.Complete();
                _isTrackingActiveHandle = false;

                var points = _resultingList.AsArray().ToArray();
                var path = new HexPath(points);
                _pathsList.AddCalculatedPath(path);
            }

            if (!_pathsList.TryGetRequestedPath(out var startEnd))
                return;

            UpdateHexNodesData(startEnd.xy);
            var job = new ConstructHexPathJob()
            {
                StartPos = startEnd.xy,
                TargetPos = startEnd.zw,
                NodesData = _nodeData,
                ResultingData = _resultingList,
                OpenedList = _openedList,
            };
            _activeHandle = job.ScheduleByRef();
            _calculatingPathStartEnd = startEnd;
            _isTrackingActiveHandle = true;
        }

        public void Dispose()
        {
            _nodeData.Dispose();
            _resultingList.Dispose();
            _openedList.Dispose();
        }

        private void UpdateHexNodesData(int2 startPos)
        {
            _nodeData.Clear();
            foreach (var hex in _map.Hexes)
            {
                var pos = hex.HexCoordinate;
                _nodeData.Add(pos, new()
                {
                    EdgesPassabilityMask = _map.GetHexEdgePassabilityMask(pos),
                    HeuristicCost = HexMath.CalculateDistance(startPos, pos),
                    Status = NavigationNodeStatus.Undefined,
                });
            }
        }
    }
}