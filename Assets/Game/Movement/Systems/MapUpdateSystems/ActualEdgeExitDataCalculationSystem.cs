using System.Buffers;
using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {

    // calculate actual edge exits data by requests and writes into UpdatedPortalExitsList
    // other system will do actualizing / registration / etc.

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ActualEdgeExitDataCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private readonly IUpdatableMap _map;
        private readonly UpdateEdgeExitsRequestsList _requests;
        private readonly HexDataCoordinator _hexDataCoordinator;
        private readonly PortalExitsUpdateDataPool _exitsUpdateDataPool;
        private readonly UpdatedPortalExitsList _updatedPortalsData;
        private readonly List<BothSideHexEdge> _clearList = new(4);

        [Inject]
        public ActualEdgeExitDataCalculationSystem(
            IUpdatableMap map, 
            UpdateEdgeExitsRequestsList requestsList, 
            HexDataCoordinator hexDataCoordinator,
            UpdatedPortalExitsList updatedPortalsData,
            PortalExitsUpdateDataPool exitUpdateDataPool)
        {
            _map = map;
            _requests = requestsList;
            _hexDataCoordinator = hexDataCoordinator;
            _updatedPortalsData = updatedPortalsData;
            _exitsUpdateDataPool = exitUpdateDataPool;
        }

        public void Dispose() { }

        public void OnAwake() { }

        public void OnUpdate(float deltaTime) 
        {
            if (!_map.IsInitialized)
                return;

            var count = _requests.Count;
            if (count == 0)
                return;

            foreach (var doubleEdgeKey in _requests)
            {
                var hexKeyA = doubleEdgeKey.SideA;
                var hexKeyB = doubleEdgeKey.SideB;

                if (_hexDataCoordinator.DoesHexRequireUpdate(hexKeyA.HexCoord) || _hexDataCoordinator.DoesHexRequireUpdate(hexKeyB.HexCoord))
                    continue;

                if (!_hexDataCoordinator.IsHexCalculated(hexKeyA.HexCoord) || !_hexDataCoordinator.IsHexCalculated(hexKeyB.HexCoord))
                    continue;

                UpdateEdgeTrianglesNeighboursMaskCommand.Execute(_map, doubleEdgeKey);

                var updateData = _exitsUpdateDataPool.Get();
                CalculateHexExitsCommand.Execute(_map, hexKeyA.HexCoord, hexKeyA.Edge, updateData.ExitsA);
                CalculateHexExitsCommand.Execute(_map, hexKeyB.HexCoord, hexKeyB.Edge, updateData.ExitsB);
                updateData.ExitsB.Reverse();

                //UnityEngine.Debug.Log($"{hexKeyA}  - {updateData.ExitsA.Count} exits");
                //UnityEngine.Debug.Log($"{hexKeyB}  - {updateData.ExitsB.Count} exits");

                _updatedPortalsData.Add(doubleEdgeKey, updateData);
                _clearList.Add(doubleEdgeKey);
            }

            if (_clearList.Count != 0)
            {
                foreach (var clearKey in _clearList)
                    _requests.Remove(clearKey);
                _clearList.Clear();
            }
        }
    }
}