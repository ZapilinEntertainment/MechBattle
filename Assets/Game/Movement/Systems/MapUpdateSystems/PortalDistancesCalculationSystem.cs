using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;
using ZE.Utils;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PortalDistancesCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private readonly PortalDistancesCalculationRequests _requests;
        private readonly IHexPortalsList _portalsList;
        private readonly IPortalExitsList _exitsList;
        private readonly HexPortalsCoordinator _portalsCoordinator;
        private readonly CalculatePointDistancesProcess _calculationProcess;

        [Inject]
        public PortalDistancesCalculationSystem(
            PortalDistancesCalculationRequests requests, 
            INavigationMap map, 
            IHexPortalsList portalsList,
            IPortalExitsList exitsList,
            HexPortalsCoordinator portalsCoordinator)
        {
            _requests = requests;
            _portalsList = portalsList;
            _portalsCoordinator = portalsCoordinator;
            _exitsList = exitsList;

            _calculationProcess = new(Allocator.Persistent, map);
        }

        public void OnAwake() { }
        public void Dispose() => _calculationProcess.Dispose();

        public void OnUpdate(float deltaTime) 
        {
            if (_requests.Count == 0)
                return;

            foreach (var portalId in _requests)
            {
                if (!_portalsList.TryGetValue(portalId, out var portal))
                {
                    _portalsCoordinator.OnPortalOutdated(portalId);
                    continue;
                }

                CalculateDistancesForPortalExits(portalId, portal);
            }

            _requests.Clear();
        }

        private void CalculateDistancesForPortalExits(int portalId, NavigationPortal portal)
        {
            if (_exitsList.TryGetValue(portal.ExitIdA, out var exitA))
                ProcessExitDistances(portalId, exitA, portal.HexCoordA);

            if (_exitsList.TryGetValue(portal.ExitIdB, out var exitB))
                ProcessExitDistances(portalId, exitB, portal.HexCoordB);
        }

        private void ProcessExitDistances(int portalId, NavigationPortalExit exit, int2 hexCoord)
        {
            var results = _calculationProcess.Run(new(portalId, hexCoord, exit.Center));
            _portalsCoordinator.ApplyPortalDistancesMap(results);
        }
    }
}