using System;
using Unity.Collections;


namespace ZE.MechBattle.Navigation
{
    public class FlowFieldCalculationCollections : IDisposable
    {
        public SquaredHexTrianglesList<TriangleNavData> SetupData;
        public NativeArray<FlowFieldCellCalculationData> CalculationData;
        public NativeQueue<int> CalculationQueue;
        public NativeHashSet<int> QueuedPositions;

        private NativeArray<TriangleNavData> _setupDataArray;
        private readonly int _hexRadius;

        public FlowFieldCalculationCollections(Allocator allocator, IntTriangularPos triangularCenterPos, int hexRadius)
        {
            _hexRadius = hexRadius;
            var coordsConverter = new TrianglesToIndexSquaredConverter(triangularCenterPos, _hexRadius);
            _setupDataArray = new NativeArray<TriangleNavData>(coordsConverter.ArrayElementsCount, allocator);
            SetupData = new SquaredHexTrianglesList<TriangleNavData>(_setupDataArray, coordsConverter);

            CalculationQueue = new NativeQueue<int>(allocator);
            var hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(_hexRadius);
            QueuedPositions = new NativeHashSet<int>(hexTrianglesCount / 2, allocator);
            CalculationData = new NativeArray<FlowFieldCellCalculationData>(SetupData.Length, allocator, NativeArrayOptions.UninitializedMemory);
        }

        public void ChangeHexPosAndReset(IntTriangularPos triangularCenterPos)
        {
            var coordsConverter = new TrianglesToIndexSquaredConverter(triangularCenterPos, _hexRadius);

            // note: setup data array is not disposed, this structure only operates it
            SetupData = new SquaredHexTrianglesList<TriangleNavData>(_setupDataArray, coordsConverter);
            for (var i = 0; i < _setupDataArray.Length; i++)
            {
                SetupData[i] = default;
            }

            CalculationQueue.Clear();
            QueuedPositions.Clear();

            for (var i = 0; i < CalculationData.Length; i++)
            {
                CalculationData[i] = default;
            }
        }

        public void Dispose()
        {
            _setupDataArray.Dispose();
            CalculationData.Dispose();
            CalculationQueue.Dispose();
            QueuedPositions.Dispose();
        }
    }
}
