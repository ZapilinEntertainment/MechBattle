using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct FlattenedHexCoordsConverter
    {
        public readonly int TrianglesPerSector;

        private readonly float _hexEdgeLength;
        private readonly float _triangleHeight;
        private readonly int _hexRadius;

        private readonly TrianglesToIndexFlattenedConverter TopConverter;
        private readonly TrianglesToIndexFlattenedConverter TopRightConverter;
        private readonly TrianglesToIndexFlattenedConverter BottomRightConverter;
        private readonly TrianglesToIndexFlattenedConverter BottomConverter;
        private readonly TrianglesToIndexFlattenedConverter BottomLeftConverter;
        private readonly TrianglesToIndexFlattenedConverter TopLeftConverter;

        /// <returns> row indices table native array </returns>
        public static IDisposable CreateCoordsConverter(
            Allocator allocator, 
            IntTriangularPos hexCenter, 
            in MapSettings mapSettings, 
            out FlattenedHexCoordsConverter converter)
        {
            var hexRadius = mapSettings.TrianglesPerHexEdge;
            var rowIndicesTable = TrianglesToIndexFlattenedConverter.FulfilRowIndices(allocator, hexRadius);
            converter = new(hexCenter, hexRadius, mapSettings.HexEdgeSize, mapSettings.TriangleHeight, rowIndicesTable.AsReadOnly());
            return rowIndicesTable;
        }

        public FlattenedHexCoordsConverter(
            IntTriangularPos hexCenter, 
            int hexRadius, 
            float hexEdgeLength,
            float triangleHeight,
            NativeArray<byte>.ReadOnly rowIndicesTable)
        {
            _hexRadius = hexRadius;
            _hexEdgeLength = hexEdgeLength;
            _triangleHeight = triangleHeight;

            TopConverter = new(new(hexCenter.X, hexCenter.Y + 1, hexCenter.Z), hexRadius, rowIndicesTable);
            TopRightConverter = CreateSectorConverter(HexSector.TopRight);
            BottomRightConverter = CreateSectorConverter(HexSector.BottomRight);
            BottomConverter = CreateSectorConverter(HexSector.Bottom);
            BottomLeftConverter = CreateSectorConverter(HexSector.BottomLeft);
            TopLeftConverter = CreateSectorConverter(HexSector.TopLeft);

            TrianglesPerSector = hexRadius * hexRadius;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            TrianglesToIndexFlattenedConverter CreateSectorConverter(HexSector sector)
            {
                var edge = (HexEdge)sector;
                var pinnaclePos =  sector.GetPinnaclePos(hexCenter + edge.ToTriangleOffsetVector(), hexRadius);
                return new TrianglesToIndexFlattenedConverter(pinnaclePos, hexRadius, rowIndicesTable);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetIndexOffset(HexSector sector) => (int)sector * TrianglesPerSector;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (HexSector sector, int localIndex) GetLocalIndex(int globalIndex)
        {
            var sectorIndex = math.clamp(globalIndex / TrianglesPerSector, 0, 5);
            return ((HexSector)sectorIndex, globalIndex - sectorIndex * TrianglesPerSector);
        }

        public int TriangularToIndex(IntTriangularPos pos)
        {
            var sector = TriangularMath.DefineSector(pos, _hexEdgeLength, _hexRadius, _triangleHeight);
            switch (sector)
            {
                case HexSector.TopRight: return TopRightConverter.TriangularToIndex(pos) + GetIndexOffset(sector);
                case HexSector.BottomRight: return BottomRightConverter.TriangularToIndex(pos) + GetIndexOffset(sector);
                case HexSector.Bottom: return BottomConverter.TriangularToIndex(pos) + GetIndexOffset(sector);
                case HexSector.BottomLeft: return BottomLeftConverter.TriangularToIndex(pos) + GetIndexOffset(sector);
                case HexSector.TopLeft: return TopLeftConverter.TriangularToIndex(pos) + GetIndexOffset(sector);
                default: return TopConverter.TriangularToIndex(pos);
            }
        }

        public bool TryGetIndex(IntTriangularPos pos, out int index)
        {
            var sector = TriangularMath.DefineSector(pos, _hexEdgeLength, _hexRadius, _triangleHeight);
            index = -1;
            var isSuccess = false;
            switch (sector)
            {
                case HexSector.TopRight: isSuccess = TopRightConverter.TryGetIndex(pos, out index); break;
                case HexSector.BottomRight: isSuccess = BottomRightConverter.TryGetIndex(pos, out index); break;
                case HexSector.Bottom: isSuccess = BottomConverter.TryGetIndex(pos, out index); break;
                case HexSector.BottomLeft: isSuccess = BottomLeftConverter.TryGetIndex(pos, out index); break;
                case HexSector.TopLeft: isSuccess = TopLeftConverter.TryGetIndex(pos, out index); break;
                default: isSuccess = TopConverter.TryGetIndex(pos, out index); break;
            }
            index += GetIndexOffset(sector);
            return isSuccess;
        }

        public IntTriangularPos IndexToTriangular(int index)
        {
            var data = GetLocalIndex(index);
            switch (data.sector)
            {
                case HexSector.TopRight: return TopRightConverter.IndexToTriangular(data.localIndex);
                case HexSector.BottomRight: return BottomRightConverter.IndexToTriangular(data.localIndex);
                case HexSector.Bottom: return BottomConverter.IndexToTriangular(data.localIndex);
                case HexSector.BottomLeft: return BottomLeftConverter.IndexToTriangular(data.localIndex);
                case HexSector.TopLeft: return TopLeftConverter.IndexToTriangular(data.localIndex);
                default: return TopConverter.IndexToTriangular(data.localIndex);
            }
        }

        public bool TryGetTriangular(int index, out IntTriangularPos pos)
        {
            var data = GetLocalIndex(index);
            switch (data.sector)
            {
                case HexSector.TopRight: return TopRightConverter.TryGetTriangular(data.localIndex, out pos);
                case HexSector.BottomRight: return BottomRightConverter.TryGetTriangular(data.localIndex, out pos);
                case HexSector.Bottom: return BottomConverter.TryGetTriangular(data.localIndex, out pos);
                case HexSector.BottomLeft: return BottomLeftConverter.TryGetTriangular(data.localIndex, out pos);
                case HexSector.TopLeft: return TopLeftConverter.TryGetTriangular(data.localIndex, out pos);
                default: return TopConverter.TryGetTriangular(data.localIndex, out pos);
            }
        }

        public FlattenedHexCoordsConverter ChangeHexCenter(IntTriangularPos hexCenter) =>
            new(hexCenter, _hexRadius, _hexEdgeLength, _triangleHeight, TopConverter.GetRowIndicesTable());
    }
}
