using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public record VirtualHex 
    {
        public readonly bool DefaultPassability;
        protected readonly HexEdgesAccessMap _accessMap;
        protected readonly INavigationMap _map;
        

        public VirtualHex(INavigationMap map, HexEdgesAccessMap accessMap, bool defaultPassability)
        {
            _map = map;
            _accessMap = accessMap;
            DefaultPassability = defaultPassability;
        }

        public bool IsCellPassable(IntTriangularPos pos) => DefaultPassability;
        public HexEdgesAccessMap GetAccessMap() => _accessMap;

        public virtual CombinedFlowData GetCombinedCellData(IntTriangularPos pos) =>
            CombinedFlowData.CreateDefaultCell(pos, _map);

        public static VirtualHex CreateFullPassableMap(INavigationMap map) => new(map, HexEdgesAccessMap.FullAccessMap, true);
        public static VirtualHex CreateFullBlockedMap(INavigationMap map) => new(map, HexEdgesAccessMap.NoWayMap, false);

       
    }
}
