using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ZE.MechBattle.Navigation
{
    public class CombinedFlowMapCellsStorage : IDisposable
    {
        public readonly int SingleLength;
        public readonly FlattenedHexCoordsConverter CoordsConverter;
        private readonly DisposableArray<int> _values;
        

        public CombinedFlowMapCellsStorage(int singleMapCellsCount, in FlattenedHexCoordsConverter coordsConverter)
        {
            SingleLength = singleMapCellsCount;
            CoordsConverter = coordsConverter;
            _values = new DisposableArray<int>(SingleLength * 6);
        }

        public void SetValue(HexEdge edge, int index, FlowMapCellData cellData) 
        {
            _values[ToLocalIndex(edge, index)] = cellData.Value;
        }


        public FlowMapCellData GetValue(int edge, int index) =>
            new(_values[ToLocalIndex(edge, index)]);

        public FlowMapCombinedCell GetCombinedCell(int index, CellPassabilityData triangleData) =>
            new(
                _values[ToLocalIndex(0,index)],
                _values[ToLocalIndex(1, index)],
                _values[ToLocalIndex(2, index)],
                _values[ToLocalIndex(3, index)],
                _values[ToLocalIndex(4, index)],
                _values[ToLocalIndex(5, index)],
                triangleData);

        public FlowMapCombinedCell GetCombinedCell(IntTriangularPos tripos, CellPassabilityData triangleData) => 
            GetCombinedCell(CoordsConverter.TriangularToIndex(tripos), triangleData);

        public void Dispose()
        {
            _values.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ToLocalIndex(int edge, int index) => edge * SingleLength + index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int ToLocalIndex(HexEdge edge, int index) => ToLocalIndex((int)edge, index);
    }
}
