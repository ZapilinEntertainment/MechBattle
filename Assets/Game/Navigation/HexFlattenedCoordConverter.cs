using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexFlattenedCoordConverter
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


        public HexFlattenedCoordConverter(
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
            switch (sector)
            {
                case HexSector.TopRight: return TopRightConverter.TryGetIndex(pos, out index);
                case HexSector.BottomRight: return BottomRightConverter.TryGetIndex(pos, out index);
                case HexSector.Bottom: return BottomConverter.TryGetIndex(pos, out index);
                case HexSector.BottomLeft: return BottomLeftConverter.TryGetIndex(pos, out index);
                case HexSector.TopLeft: return TopLeftConverter.TryGetIndex(pos, out index);
                default: return TopConverter.TryGetIndex(pos, out index);
            }
        }

        public IntTriangularPos IndexToTriangular(int index)
        {
            var localIndex = GetLocalIndex(index);
            switch (localIndex.sector)
            {
                case HexSector.TopRight: return TopRightConverter.IndexToTriangular(index);
                case HexSector.BottomRight: return BottomRightConverter.IndexToTriangular(index);
                case HexSector.Bottom: return BottomConverter.IndexToTriangular(index);
                case HexSector.BottomLeft: return BottomLeftConverter.IndexToTriangular(index);
                case HexSector.TopLeft: return TopLeftConverter.IndexToTriangular(index);
                default: return TopConverter.IndexToTriangular(index);
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
    }
}
