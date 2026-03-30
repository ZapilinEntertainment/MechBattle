using System;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class TriangularPathJobCollections : IDisposable
    {
        public SquaredHexTrianglesList<TriangleNavData> SetupData;
        public NativeArray<AstarPathNodeData<IntTriangularPos>> CalculationData;
        public NativeList<IntTriangularPos> ResultList;
        public NativeHashSet<int> OpenedList;

        public TriangularPathJobCollections(Allocator allocator, NavigationHexPosition hexPos, int hexRadius)
        {
            var trisCount = TriangularMath.GetTrianglesCountInHex(hexRadius);
            SetupData = new(hexPos.TriangularCenterPos, hexRadius, allocator);
            CalculationData = new(SetupData.Length, allocator);
            ResultList = new(trisCount, allocator);
            OpenedList = new(trisCount-1, allocator);
        }

        public void Dispose()
        {
            SetupData.Dispose();
            CalculationData.Dispose();
            ResultList.Dispose();
            OpenedList.Dispose();
        }
    }
}
