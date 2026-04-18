using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public struct FlattenedHexList<T> where T : unmanaged
    {
        public int Length => _array.Length;

        private readonly FlattenedHexCoordsConverter _coordsConverter;
        private NativeArray<T> _array;

        public FlattenedHexList(in FlattenedHexCoordsConverter coordsConverter, NativeArray<T> array)
        {
            _coordsConverter = coordsConverter;
            _array = array;
        }
    
        public T this[int index]
        {
            get => _array[index];
            set => _array[index] = value;
        }

        public T this[IntTriangularPos pos]
        {
            get => _array[_coordsConverter.TriangularToIndex(pos)];
            set => _array[_coordsConverter.TriangularToIndex(pos)] = value;
        }

        public bool TryGetValue(IntTriangularPos pos, out T value)
        {
            var isValidIndex =_coordsConverter.TryGetIndex(pos, out var index);
            value = isValidIndex ? _array[index] : default(T);
            return isValidIndex;
        }

        public bool TryGetValue(IntTriangularPos pos, out T value, out int index)
        {
            var isValidIndex = _coordsConverter.TryGetIndex(pos, out index);
            value = isValidIndex ? _array[index] : default(T);
            return isValidIndex;
        }

        public FlattenedHexCoordsConverter GetCoordsConverter() => _coordsConverter;

        public IntTriangularPos IndexToTriangular(int index) => _coordsConverter.IndexToTriangular(index);
        public int TriangularToIndex(IntTriangularPos pos) => _coordsConverter.TriangularToIndex(pos);

        public bool TryGetIndex(IntTriangularPos pos, out int index) => _coordsConverter.TryGetIndex(pos, out index);

        public FlattenedHexList<T> ChangeHexCenter(IntTriangularPos newHexCenter) => new(_coordsConverter.ChangeHexCenter(newHexCenter), _array);
    }
}
