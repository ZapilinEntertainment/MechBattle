using System;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public interface IFlowMap
    {
        static readonly IFlowMap NoWay = new NoWayFlowMap();
        static readonly IFlowMap FullAccess = new FullAccessFlowMap();

        bool IsStub { get;}
        FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos);
        HexEdgesAccessMap GetAccessMap();
    }

    public interface IDisposableFlowMap : IFlowMap, IDisposable { }

    public class NoWayFlowMap : IFlowMap
    {
        public bool IsStub => true;
        public HexEdgesAccessMap GetAccessMap() => HexEdgesAccessMap.NoWayMap;

        public FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos) => FlowMapCombinedCell.CreateDefaultCell(pos,false);
    }

    public class FullAccessFlowMap : IFlowMap
    {
        public bool IsStub => true;
        public HexEdgesAccessMap GetAccessMap() => HexEdgesAccessMap.FullAccessMap;

        public FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos) => FlowMapCombinedCell.CreateDefaultCell(pos, true);
    }

    public class StubFlowMap : IDisposableFlowMap
    {
        public bool IsStub => true;
        private readonly HexEdgesAccessMap _accessMap;

        public StubFlowMap(HexEdgesAccessMap accessMap)
        {
            _accessMap = accessMap;
        }

        public HexEdgesAccessMap GetAccessMap() => _accessMap;

        public FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos) => FlowMapCombinedCell.CreateDefaultCell(pos,true);


        public void Dispose() { }
    }
}
