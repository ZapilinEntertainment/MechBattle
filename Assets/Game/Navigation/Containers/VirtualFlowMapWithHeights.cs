using System.Collections.Generic;

namespace ZE.MechBattle.Navigation
{
    public record VirtualFlowMapWithHeights : VirtualFlowMap
    {
        public override FlowMapType Type => FlowMapType.VirtualWithRealHeights;
        private readonly Dictionary<IntTriangularPos, short> _heights;

        public VirtualFlowMapWithHeights(
            INavigationMap map, 
            HexEdgesAccessMap accessMap, 
            bool defaultPassability, 
            IReadOnlyDictionary<IntTriangularPos, TriangleNavData> trianglesData)
            : base(map, accessMap, defaultPassability)
        {
            _heights = new();
            foreach (var kvp in trianglesData)
            {
                _heights.Add(kvp.Key, kvp.Value.Height);
            }
        }

        public VirtualFlowMapWithHeights(
            INavigationMap map,
            HexEdgesAccessMap accessMap,
            bool defaultPassability,
            Dictionary<IntTriangularPos, short> heights)
            : base(map, accessMap, defaultPassability)
        {
            _heights = heights;
        }

        public override FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos)
        {
            return FlowMapCombinedCell.CreateDefaultCell(
                pos,
                TriangleNavData.CreateDefaultData(DefaultPassability, GetHeight(pos)),
                _map);
        }

        public override short GetHeight(IntTriangularPos pos) => 
            _heights.TryGetValue(pos, out var height) ? height : NavigationConstants.DEFAULT_HEIGHT;
    }
}
