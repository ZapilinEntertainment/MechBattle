using System;

namespace ZE.MechBattle.Navigation
{
    public enum FlowMapType : byte
    {
        Virtual,
        VirtualWithRealHeights,
        Calculated,
    }

    public interface IFlowMap
    {
        FlowMapType Type { get;}

        bool IsCellPassable(IntTriangularPos pos);
        short GetHeight(IntTriangularPos pos);
        FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos);
        HexEdgesAccessMap GetAccessMap();
    }

    public interface IDisposableFlowMap : IFlowMap, IDisposable { }   
}
