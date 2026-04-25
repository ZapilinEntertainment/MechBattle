using Unity.Collections;
using Unity.Burst;
using System;

namespace ZE.MechBattle.Navigation
{
    public static class FormHexAccessMapCommand
    {

        [BurstCompile]
        public static HexEdgesAccessMap Execute(
            FlowFieldCalculationCollections collections,
            NavigationHexPosition hex, 
            int trianglesPerEdge)
        {
            Span<uint> combinedMask = stackalloc uint[6];
            for (var i = 0; i < 6; i++)
            {
                combinedMask[i] = FormEdgeAccessMask(i, collections, hex, trianglesPerEdge);
            }

            var bitfield = new HexEdgesAccessMap().Data;
            for (var i = 0; i < 6; i++)
            {
                var startEdge = (HexEdge)i;
                for (var j = 0; j < 6; j++)
                {
                    if (j == i) continue;
                    var endEdge = (HexEdge)j;
                    var connectionIndex = HexEdgesAccessMap.GetConnectionIndex(startEdge, endEdge);
                    bitfield.SetBits(connectionIndex, (combinedMask[j] & (1 << i)) != 0);
                }
            }
            return new HexEdgesAccessMap(bitfield);
        }

        private static uint FormEdgeAccessMask(
            int edgeIndex,
            FlowFieldCalculationCollections collections,
            NavigationHexPosition hex,
            int trianglesPerEdge)
        {
            switch ((HexEdge)edgeIndex)
            {
                case HexEdge.TopRight: return FormEdgeAccessData<TopRightEdgeEnumerationLogic>(new(trianglesPerEdge, hex), collections); 
                case HexEdge.BottomRight: return FormEdgeAccessData<BottomRightEdgeEnumerationLogic>(new(trianglesPerEdge, hex), collections); 
                case HexEdge.Bottom: return FormEdgeAccessData<BottomEdgeEnumerationLogic>(new(trianglesPerEdge, hex), collections); 
                case HexEdge.BottomLeft: return FormEdgeAccessData<BottomLeftEdgeEnumerationLogic>(new(trianglesPerEdge, hex), collections); 
                case HexEdge.TopLeft: return FormEdgeAccessData<TopLeftEdgeEnumerationLogic>(new(trianglesPerEdge, hex), collections); 
                default: return FormEdgeAccessData<TopEdgeEnumerationLogic>(new(trianglesPerEdge, hex), collections); 
            }
        }

        private static uint FormEdgeAccessData<T>(EdgeEnumerator<T> enumerator, FlowFieldCalculationCollections collections) where T : struct, IEdgeEnumerationLogic
        {
            uint combinedAccessMask = 0;

            foreach (var pos in enumerator)
            {
                combinedAccessMask |=  collections.GetFlowData(pos).GetCombinedEdgeAccessMask();
            }

            return combinedAccessMask;
        }
    
    }
}
