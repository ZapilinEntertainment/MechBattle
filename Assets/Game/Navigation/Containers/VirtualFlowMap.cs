using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public record VirtualFlowMap : IDisposableFlowMap
    {
        public virtual FlowMapType Type => FlowMapType.Virtual;
        public readonly bool DefaultPassability;
        protected readonly HexEdgesAccessMap _accessMap;
        protected readonly INavigationMap _map;
        

        public VirtualFlowMap(INavigationMap map, HexEdgesAccessMap accessMap, bool defaultPassability)
        {
            _map = map;
            _accessMap = accessMap;
            DefaultPassability = defaultPassability;
        }

        public bool IsCellPassable(IntTriangularPos pos) => DefaultPassability;
        public HexEdgesAccessMap GetAccessMap() => _accessMap;

        public virtual FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos) =>
            FlowMapCombinedCell.CreateDefaultCell(
                pos,
                CellPassabilityData.CreateDefaultData(DefaultPassability),
                _map);

        public void Dispose() { }

        public static VirtualFlowMap CreateFullPassableMap(INavigationMap map) => new(map, HexEdgesAccessMap.FullAccessMap, true);
        public static VirtualFlowMap CreateFullBlockedMap(INavigationMap map) => new(map, HexEdgesAccessMap.NoWayMap, false);

       
    }
}
