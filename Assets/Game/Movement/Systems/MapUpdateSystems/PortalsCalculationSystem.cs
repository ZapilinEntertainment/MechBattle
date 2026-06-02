using System.Collections.Generic;
using Scellecs.Morpeh;
using VContainer;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PortalsCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private readonly INavigationMap _map;
        private readonly PortalCalculationRequestsList _requests;
        private readonly List<NavigationPortal> _portalList = new();
        private const int MAX_OPERATIONS_PER_TICK = 4;

        [Inject]
        public PortalsCalculationSystem(INavigationMap map, PortalCalculationRequestsList requestsList)
        {
            _map = map;
            _requests = requestsList;
        }

        public void Dispose()
        {

        }

        public void OnAwake() 
        {

        }

        public void OnUpdate(float deltaTime) 
        {
            var count = _requests.Count;
            if (count == 0)
                return;

            var iterationsDone = 0;
            foreach (var request in _requests)
            {
                _portalList.Clear();

                HandleRequest(request);

                // add to both list

                iterationsDone++;
                if (iterationsDone == MAX_OPERATIONS_PER_TICK)
                    break;
            }
        }

        private void HandleRequest(HexUpdateRequest request)
        {
            FormPortalsCommand.Execute(_map, request.HexCoord, HexEdge.Top, _portalList);
            FormPortalsCommand.Execute(_map, request.HexCoord, HexEdge.TopRight, _portalList);
            FormPortalsCommand.Execute(_map, request.HexCoord, HexEdge.BottomRight, _portalList);
            FormPortalsCommand.Execute(_map, request.HexCoord, HexEdge.Bottom, _portalList);
            FormPortalsCommand.Execute(_map, request.HexCoord, HexEdge.BottomLeft, _portalList);
            FormPortalsCommand.Execute(_map, request.HexCoord, HexEdge.TopLeft, _portalList);

            // todo: update all 7 hexes
        }
    }
}