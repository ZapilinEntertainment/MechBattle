using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class TriangularPathJobCollections : IDisposable, IReadOnlyList<IntTriangularPos>
    {
        public NativeArray<AstarPathNodeData<IntTriangularPos>> CalculationData { get; private set; }
        public NativeList<IntTriangularPos> ResultList { get; private set; }
        public NativeHashSet<int> OpenedList { get; private set; }
        public ref FlattenedHexList<CellPassabilityData> PassabilityData => ref _setupData;

        private FlattenedHexList<CellPassabilityData> _setupData;
        private readonly NativeArray<CellPassabilityData> _setupDataArray;
        private readonly IDisposable _rowIndicesArray;

        public TriangularPathJobCollections(Allocator allocator, NavigationHexPosition hexPos, in MapSettings mapSettings)
        {
            var hexRadius = mapSettings.TrianglesPerHexEdge;
            var trisCount = TriangularMath.GetTrianglesCountInHex(hexRadius);            
            ResultList = new(trisCount, allocator);
            OpenedList = new(trisCount-1, allocator);

            _rowIndicesArray = FlattenedHexCoordsConverter.CreateCoordsConverter(allocator, hexPos.TriangularCenterPos, mapSettings, out var coordsConverter);
            _setupDataArray = new NativeArray<CellPassabilityData>(TriangularMath.GetTrianglesCountInHex(hexRadius), allocator);
            _setupData = new FlattenedHexList<CellPassabilityData>(coordsConverter, _setupDataArray);

            CalculationData = new(_setupData.Length, allocator);
        }

        public void ChangeCenter(NavigationHexPosition pos) =>
            _setupData = _setupData.ChangeHexCenter(pos.TriangularCenterPos);

        public void Dispose()
        {
            _setupDataArray.Dispose();
            CalculationData.Dispose();
            ResultList.Dispose();
            OpenedList.Dispose();
            _rowIndicesArray.Dispose();
        }

        #region IReadonlyList of IntTriangularPos
        public int Count => ResultList.Length;

        public IntTriangularPos this[int index] => ResultList[index];
        public IEnumerator<IntTriangularPos> GetEnumerator() => ResultList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ResultList.GetEnumerator();
        #endregion
    }
}
