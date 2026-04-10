using System;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class TriangularPathJobCollections : IDisposable
    {
        public SquaredHexTrianglesList<TriangleNavData> SetupData { get; private set; }
        public NativeArray<AstarPathNodeData<IntTriangularPos>> CalculationData { get; private set; }
        public NativeList<IntTriangularPos> ResultList { get; private set; }
        public NativeHashSet<int> OpenedList { get; private set; }

        private readonly NativeArray<TriangleNavData> _setupDataArray;

        public TriangularPathJobCollections(Allocator allocator, NavigationHexPosition hexPos, int hexRadius)
        {
            var trisCount = TriangularMath.GetTrianglesCountInHex(hexRadius);            
            ResultList = new(trisCount, allocator);
            OpenedList = new(trisCount-1, allocator);

            var coordsConverter = new TrianglesToIndexSquaredConverter(hexPos.TriangularCenterPos, hexRadius);
            _setupDataArray = new NativeArray<TriangleNavData>(coordsConverter.ArrayElementsCount, allocator);
            SetupData = new SquaredHexTrianglesList<TriangleNavData>(_setupDataArray, coordsConverter);

            CalculationData = new(SetupData.Length, allocator);
        }

        public void ChangeCenter(NavigationHexPosition pos)
        {
            var newConverter = new TrianglesToIndexSquaredConverter(pos.TriangularCenterPos, SetupData.CoordsConverter.HexRadius);
            SetupData = new(_setupDataArray, newConverter);
        }

        public void Dispose()
        {
            _setupDataArray.Dispose();
            CalculationData.Dispose();
            ResultList.Dispose();
            OpenedList.Dispose();
        }
    }
}
