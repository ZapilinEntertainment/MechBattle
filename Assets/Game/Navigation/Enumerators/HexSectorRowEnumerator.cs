namespace ZE.MechBattle.Navigation
{
    public struct HexSectorRowEnumerator
    {
        public IntTriangularPos Current { get; private set; }

        public readonly HexSectorRowsEnumerationSetup EnumerationSetup;

        private int _index;
        private readonly int _trisCount;

        public HexSectorRowEnumerator(HexSector sector, IntTriangularPos pinnacleTripos) : this(new(sector, pinnacleTripos), 0) { }

        public HexSectorRowEnumerator(HexSectorRowsEnumerationSetup enumerationSetup, int rowIndex)
        {
            EnumerationSetup = enumerationSetup;

            if (rowIndex == 0)
            {
                Current = EnumerationSetup.Pinnacle;   
            }
            else
            {
                var dir = EnumerationSetup.IsStartTrianglePeak ? EnumerationSetup.PeakRowOffset.ToTriangularOffsetVector() : EnumerationSetup.ValleyRowOffset.ToTriangularOffsetVector();
                Current = EnumerationSetup.Pinnacle + dir * rowIndex;
            }

            _index = -1;
            _trisCount = rowIndex * 2 + 1;
        }

        public bool MoveNext()
        {
            if (_index == -1)
            {
                _index = 0;
                return true;
            }

            if (Current.IsPeak)
                Current = TriangularMath.GetPeakNeighbour(Current, EnumerationSetup.PeakColumnOffset);
            else
                Current = TriangularMath.GetValleyNeighbour(Current, EnumerationSetup.ValleyColumnOffset);
            _index++;
            return _index < _trisCount;
        }

        public HexSectorRowEnumerator GetEnumerator() => this;
    }
}
