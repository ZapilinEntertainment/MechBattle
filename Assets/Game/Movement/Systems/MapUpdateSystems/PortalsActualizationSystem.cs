using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class PortalsActualizationSystem : ISystem
    {
        public World World { get;set;}
        
        private readonly UpdatePortalRequestsList _portalRequestsList;
        private readonly PortalsUpdateHandler _logic;

        [Inject]
        public PortalsActualizationSystem(
            UpdatePortalRequestsList requestsList, 
            INavigationMap map, 
            HexPortalsCoordinator portalsCoordinator,
            HexPortalsList portalsList,
            PortalExitsList exitsList)
        {
            
            _portalRequestsList = requestsList;
            _logic = new(map, portalsCoordinator, portalsList, exitsList);
        }

        public void OnAwake() { }

        public void Dispose() { }

       

        public void OnUpdate(float deltaTime) 
        { 
            if (_portalRequestsList.Count == 0)
                return;

            // 1. exits was already updated in ActualEdgeExitDataCalculationSystem + PortalEdgeExitsUpdateSystem
            // 2. but portals data has already old exits id
            // 3. so we construct old portal data mask (_existingPortalsMask) and new updated version (of freshly updated exits, _updatedPortalsMask)
            // 4. and then check both masks, making portals obsolete or register new in progress

            foreach (var request in _portalRequestsList)
            {
                _logic.Handle(request.SideA.HexCoord, request.SideA.Edge, request.SideB.HexCoord, request.SideB.Edge);
            }

            _portalRequestsList.Clear();
        }

    
    }
}
