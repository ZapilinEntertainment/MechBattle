using Unity.Mathematics;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct SubtrianglesCoordsEnumerator
    {
        private readonly IntTriangularPos _pinnaclePos;
        private readonly int _trianglesPerEdge;
        private readonly int _totalTrianglesCount;
        private readonly int3 _verticalOffsetVector;
        private readonly int _horizontalNeighbourIndexPeak;
        private readonly int _horizontalNeighbourIndexValley;
        private readonly TriangleEnumerationSettings _settings;

        private int _index;
        private int _row;
        private int _column;
        private bool _operatingPeaks;

        

        public IntTriangularPos Current { get;private set; }

        public SubtrianglesCoordsEnumerator(IntTriangularPos pinnaclePos, int trianglesPerEdge)
        {
            _pinnaclePos = pinnaclePos;
            _trianglesPerEdge = trianglesPerEdge;
            _totalTrianglesCount = _trianglesPerEdge * _trianglesPerEdge;

            _verticalOffsetVector = _pinnaclePos.IsPeak ? new(0, -1, 1) : new(0,1,-1);
            _horizontalNeighbourIndexPeak = (int)(_pinnaclePos.IsPeak ? PeakNeighbour.EdgeUpLeft : PeakNeighbour.EdgeUpRight);
            _horizontalNeighbourIndexValley = (int)(_pinnaclePos.IsPeak ? ValleyNeighbour.EdgeDownLeft : ValleyNeighbour.EdgeDownRight);

            // #setup defaults
            _index = -1;
            _row = 0;
            _column = 0;
            _operatingPeaks = true;
            //

            _settings = new(_pinnaclePos, trianglesPerEdge);
            Current = new(_settings.GetStart(true));
        }
        

        public bool MoveNext()
        {
            _index++;            

            if (_index < _totalTrianglesCount)
            {
                var typedIndex = _settings.GetTypedIndex(_index, out var isPeak);
                if (isPeak != _operatingPeaks)
                {
                    _operatingPeaks = false;
                    _row = 0;
                    _column = 0;
                }

                if (typedIndex == (_row + 1) * (_row + 2) / 2)
                {
                    _row++;
                    _column = 0;
                }
                
                Current = _settings.V2ToTriangular(new(_column, _row), isPeak);
                //UnityEngine.Debug.Log($"index: {_index} typed: {typedIndex} column: {_column} row: {_row} pos: {Current}");
                _column++;
                return true;
            }            
            return false;
        }

        public void Reset() 
        {
            // #setup defaults
            _index = -1;
            _row = 0;
            _column = 0;
            _operatingPeaks = true;
            //
        }
        public void Dispose() { }

        public SubtrianglesCoordsEnumerator GetEnumerator() => this;
    }
}
