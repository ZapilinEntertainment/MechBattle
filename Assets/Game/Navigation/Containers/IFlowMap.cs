using System;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface IFlowMap
    {
        bool IsStub { get;}

        bool IsCellPassable(IntTriangularPos pos);
        FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos);
        HexEdgesAccessMap GetAccessMap();
    }

    public interface IDisposableFlowMap : IFlowMap, IDisposable { }

    public class StubFlowMap : IDisposableFlowMap
    {
        public bool IsStub => true;
        private readonly HexEdgesAccessMap _accessMap;
        private readonly INavigationMap _map;
        private readonly bool _defaultPassability;

        public StubFlowMap(INavigationMap map, HexEdgesAccessMap accessMap, bool defaultPassability)
        {
            _map = map;
            _accessMap = accessMap;
            _defaultPassability = defaultPassability;
        }


        public bool IsCellPassable(IntTriangularPos pos) => _defaultPassability;
        public HexEdgesAccessMap GetAccessMap() => _accessMap;

        public FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos) =>
            FlowMapCombinedCell.CreateDefaultCell(
                pos,
                TriangleNavData.CreateDefaultData(_defaultPassability),
                _map);

        public void Dispose() { }

        public static StubFlowMap CreateFullPassableMap(INavigationMap map) => new(map, HexEdgesAccessMap.FullAccessMap, true);
        public static StubFlowMap CreateFullBlockedMap(INavigationMap map) => new(map, HexEdgesAccessMap.NoWayMap, false);
    }
}
