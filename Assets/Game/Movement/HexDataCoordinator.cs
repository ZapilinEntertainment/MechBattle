using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class HexDataCoordinator
    {
        private readonly INavigationMap _map;
        private readonly HexRaycastRequestsList _hexRaycastRequests;

        [Inject]
        public HexDataCoordinator(HexRaycastRequestsList hexRaycastRequests, INavigationMap map)
        {
            _hexRaycastRequests = hexRaycastRequests;
            _map = map;
        }

        public bool DoesHexRequireUpdate(int2 hexCoord) => _hexRaycastRequests.Contains(hexCoord);

        public bool IsHexCalculated(int2 hexCoord) => _map.GetOrCreateHex(hexCoord).PassabilityVersion != 0;
    
    }
}
