using System;
using Unity.Collections;


namespace ZE.MechBattle.Navigation
{
    public class FlowFieldCalculationCollections : IDisposable
    {
        public ref FlattenedHexList<CellPassabilityData> PassabilityData => ref _passabilityData;
        public NativeArray<FlowFieldCellCalculationData> CalculationData;
        public NativeQueue<int> CalculationQueue;
        public NativeHashSet<int> QueuedPositions;

        public readonly NativeArray<CellPassabilityData> PassabilityDataInnerArray;
        private readonly int _hexRadius;
        private readonly NativeArray<byte> _rowIndices;
        private FlattenedHexList<CellPassabilityData> _passabilityData;

        public static FlowFieldCalculationCollections CreateCollection(
            Allocator allocator,
            NavigationHexPosition hexPos, 
            in MapSettings mapSettings) =>
         new FlowFieldCalculationCollections(allocator, hexPos.TriangularCenterPos, mapSettings);

        public FlowFieldCalculationCollections(
            Allocator allocator,
            IntTriangularPos hexCenter, 
            in MapSettings mapSettings)
        {
            _hexRadius = mapSettings.TrianglesPerHexEdge;

            _rowIndices = TrianglesToIndexFlattenedConverter.FulfilRowIndices(allocator, _hexRadius);
            var coordsConverter = new FlattenedHexCoordsConverter(hexCenter, mapSettings.TrianglesPerHexEdge, mapSettings.HexEdgeSize, mapSettings.TriangleHeight, _rowIndices.AsReadOnly());
            PassabilityDataInnerArray = new NativeArray<CellPassabilityData>(TriangularMath.GetTrianglesCountInHex(_hexRadius), allocator);
            _passabilityData = new FlattenedHexList<CellPassabilityData>(coordsConverter, PassabilityDataInnerArray);

            CalculationQueue = new NativeQueue<int>(allocator);
            var hexTrianglesCount = TriangularMath.GetTrianglesCountInHex(_hexRadius);
            QueuedPositions = new NativeHashSet<int>(hexTrianglesCount / 2, allocator);
            CalculationData = new NativeArray<FlowFieldCellCalculationData>(_passabilityData.Length, allocator, NativeArrayOptions.UninitializedMemory);
        }

        public void ChangeHexPosAndReset(IntTriangularPos newHexCenter)
        {
            // note: setup data array cannot be not disposed, this structure only operates it
            _passabilityData = _passabilityData.ChangeHexCenter(newHexCenter);
            
            for (var i = 0; i < PassabilityDataInnerArray.Length; i++)
            {
                _passabilityData[i] = default;
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
            PassabilityDataInnerArray.Dispose();
            CalculationData.Dispose();
            CalculationQueue.Dispose();
            QueuedPositions.Dispose();
            _rowIndices.Dispose();
        }


        public IntTriangularPos GetPosByIndex(int index) => PassabilityData.IndexToTriangular(index);
    }
}
