namespace ZE.MechBattle.Navigation
{
    public struct TriangleNeighboursEnumerator<T> where T : ITriangleNeighbourOffsets
    {
        private readonly IntTriangularPos _tripos;
        private readonly T _offsets;
        private int _index;
        public IntTriangularPos Current { get; private set; }

        public TriangleNeighboursEnumerator(IntTriangularPos tripos, T offsets)
        {
            _tripos = tripos;
            _offsets = offsets;
            _index = -1;
            Current = default;
        }

        public bool MoveNext()
        {
            _index++;
            if (_index >= NavigationConstants.TRIANGLE_DIRECTIONS_COUNT)
                return false;

            var offset = _offsets[_index];
            Current = offset + _tripos;
            return true;
        }

        public TriangleNeighboursEnumerator<T> GetEnumerator() => this;

    }
}
