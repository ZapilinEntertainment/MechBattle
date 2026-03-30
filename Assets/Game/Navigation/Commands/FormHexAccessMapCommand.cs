using Unity.Collections;
using Unity.Burst;
using System;

namespace ZE.MechBattle.Navigation
{
    public static class FormHexAccessMapCommand
    {
        private readonly struct EdgeAccessData
        {
            public readonly bool IsPassable;
            public readonly int AccessMask;

            public EdgeAccessData(bool isPassable, int accessMask)
            {
                IsPassable = isPassable;
                AccessMask = accessMask;
            }
        }

        [BurstCompile]
        public static HexEdgesAccessMap Execute(
            NativeHashMap<IntTriangularPos, FlowMapCombinedCell>.ReadOnly combinedCells, 
            NavigationHexPosition hex, 
            int trianglesPerEdge)
        {
            Span<EdgeAccessData> combinedMask = stackalloc EdgeAccessData[6];
            for (var i = 0; i < 6; i++)
            {
                combinedMask[i] = FormEdgeAccessMask(i, combinedCells, hex, trianglesPerEdge);
            }

            var bitfield = new HexEdgesAccessMap().Data;
            for (var i = 0; i < 6; i++)
            {
                var startEdge = (HexEdge)i;
                for (var j = 0; j < 6; j++)
                {
                    if (j == i) continue;
                    var endEdge = (HexEdge)j;
                    var connectionIndex = HexEdgesAccessMap.DecodeConnectionIndex(startEdge, endEdge);
                    bitfield.SetBits(connectionIndex, (combinedMask[j].AccessMask & (1 << i)) != 0);
                }

                bitfield.SetBits(HexEdgesAccessMap.DecodePassabilityIndex(startEdge), combinedMask[i].IsPassable);
            }
            return new HexEdgesAccessMap(bitfield);
        }

        private static EdgeAccessData FormEdgeAccessMask(
            int edgeIndex, 
            NativeHashMap<IntTriangularPos, FlowMapCombinedCell>.ReadOnly combinedCells,
            NavigationHexPosition hex,
            int trianglesPerEdge)
        {
            switch ((HexEdge)edgeIndex)
            {
                case HexEdge.TopRight: return FormEdgeAccessMask<TopRightEdgeLogic>(new(trianglesPerEdge, hex), combinedCells); 
                case HexEdge.BottomRight: return FormEdgeAccessMask<BottomRightEdgeLogic>(new(trianglesPerEdge, hex), combinedCells); 
                case HexEdge.Bottom: return FormEdgeAccessMask<BottomEdgeLogic>(new(trianglesPerEdge, hex), combinedCells); 
                case HexEdge.BottomLeft: return FormEdgeAccessMask<BottomLeftEdgeLogic>(new(trianglesPerEdge, hex), combinedCells); 
                case HexEdge.TopLeft: return FormEdgeAccessMask<TopLeftEdgeLogic>(new(trianglesPerEdge, hex), combinedCells); 
                default: return FormEdgeAccessMask<TopEdgeLogic>(new(trianglesPerEdge, hex), combinedCells); 
            }
        }

        private static EdgeAccessData FormEdgeAccessMask<T>(EdgeEnumerator<T> enumerator, NativeHashMap<IntTriangularPos, FlowMapCombinedCell>.ReadOnly combinedCells) where T : struct, IEdgeDirectionLogic
        {
            var isPassable = false;
            var combinedAccessMask = 0;

            foreach (var pos in enumerator)
            {
                if (!combinedCells.TryGetValue(pos, out var combinedCell))
                    continue;

                isPassable |= combinedCell.IsPassable;
                combinedAccessMask |= combinedCell.GetCombinedEdgeAccessMask();
            }

            return new EdgeAccessData(isPassable: isPassable, accessMask: combinedAccessMask);
        }
    
    }
}
