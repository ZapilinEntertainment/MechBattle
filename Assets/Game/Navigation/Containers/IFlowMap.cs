using System;

namespace ZE.MechBattle.Navigation
{
    public enum FlowMapType : byte
    {
        Virtual,
        Calculated,
    }

    public interface IFlowMap
    {
        FlowMapType Type { get;}

        bool IsCellPassable(IntTriangularPos pos);
        FlowMapCombinedCell GetCombinedCellData(IntTriangularPos pos);
        HexEdgesAccessMap GetAccessMap();
    }

    public interface IDisposableFlowMap : IFlowMap, IDisposable { }   
}
