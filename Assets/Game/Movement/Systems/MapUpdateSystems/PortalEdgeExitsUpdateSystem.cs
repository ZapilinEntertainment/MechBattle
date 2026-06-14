using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {

    // Applies new exit data to current exits list

    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PortalEdgeExitsUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private readonly IUpdatableMap _map;
        private readonly UpdatedPortalExitsList _updatedPortalsList;
        private readonly HexPortalsCoordinator _portalsCoordinator;
        private readonly UpdatePortalRequestsList _portalUpdateRequests;
        private readonly List<(int id, NavigationPortalExit exit)> _currentExitsA;
        private readonly List<(int id, NavigationPortalExit exit)> _currentExitsB;
        private readonly IExitsLogic _exitsLogic;

        [Inject]
        public PortalEdgeExitsUpdateSystem(
            IUpdatableMap map, 
            UpdatedPortalExitsList updatedLists, 
            HexPortalsCoordinator portalsCoordinator,
            UpdatePortalRequestsList portalUpdateRequests,
            IExitsLogic exitsLogic)
        {
            _map = map;
            _updatedPortalsList = updatedLists;
            _portalsCoordinator = portalsCoordinator;
            _portalUpdateRequests = portalUpdateRequests;
            _exitsLogic = exitsLogic;

            var expectedMaxPortalsCount = _map.TrianglesPerHexEdge / 2;
            _currentExitsA = new(expectedMaxPortalsCount);
            _currentExitsB = new (expectedMaxPortalsCount);          
        }

        public void OnAwake() { }

        public void Dispose() { }

        public void OnUpdate(float deltaTime) 
        {
            if (_updatedPortalsList.Count == 0)
                return;

            foreach (var updatedData in _updatedPortalsList)
            {
                var bothsideKey = updatedData.Key;
                ActualizeData(bothsideKey, updatedData.Value);  
                updatedData.Value.ReturnToPool();
                _portalUpdateRequests.Add(bothsideKey);
            }

            _updatedPortalsList.Clear();
        }

        private void ActualizeData(BothSideHexEdge edgeKey, PortalExitsUpdateData updated)
        {
            // get existing exits:
            var hexA = _map.GetOrCreateUpdatableHex(edgeKey.SideA.HexCoord);
            _portalsCoordinator.GetEdgeExits(hexA, edgeKey.SideA.Edge, _currentExitsA);

            var hexB = _map.GetOrCreateUpdatableHex(edgeKey.SideB.HexCoord);
            _portalsCoordinator.GetEdgeExits(hexB, edgeKey.SideB.Edge, _currentExitsB);

            // form updated ids list,
            // mark old exit ids as outdated and remove them from hex exits list ,
            // register new exits, add their id to hex exits list
            _exitsLogic.ActualizeExitsList(_currentExitsA, updated.ExitsA, hexA);
            _exitsLogic.ActualizeExitsList(_currentExitsB, updated.ExitsB, hexB);

            _currentExitsA.Clear();
            _currentExitsB.Clear();
        }
    
    }
}