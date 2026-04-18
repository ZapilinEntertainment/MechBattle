using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public struct SubtrianglesEnumerator : IEnumerator<IntTriangularPos>, IEnumerable<IntTriangularPos>
    {
        private readonly IntTriangularPos _pinnaclePos;
        private readonly int _trianglesPerEdge;
        private readonly int3 _verticalOffsetVector;
        private readonly int _horizontalNeighbourIndexPeak;
        private readonly int _horizontalNeighbourIndexValley;

        private int _index;
        private int _row;

        public IntTriangularPos Current { get;private set; }

        object IEnumerator.Current => Current;

        public SubtrianglesEnumerator(IntTriangularPos pinnaclePos, int trianglesPerEdge)
        {
            _pinnaclePos = pinnaclePos;
            _trianglesPerEdge = trianglesPerEdge;

            // same enumeration logic as in TrianglesToIndexFlattenedConversion
            _verticalOffsetVector = _pinnaclePos.IsPeak ? new(0, -1, 1) : new(0,1,-1);
            _horizontalNeighbourIndexPeak = (int)(_pinnaclePos.IsPeak ? PeakNeighbour.EdgeUpLeft : PeakNeighbour.EdgeUpRight);
            _horizontalNeighbourIndexValley = (int)(_pinnaclePos.IsPeak ? ValleyNeighbour.EdgeDownLeft : ValleyNeighbour.EdgeDownRight);

            _index = -1;
            _row = 0;
            Current = _pinnaclePos;
        }
        

        public bool MoveNext()
        {
            if (_index == -1)
            {
                _index = 0;
                return true;
            }

            _index++;
            if (_index == (_row+1) * (_row+1))
            {
                _row++;
                if (_row == _trianglesPerEdge)
                    return false;

                Current = _pinnaclePos + _row * _verticalOffsetVector;
                return true;
            }
            else
            {
                var nextPos = Current.IsPeak 
                    ? TriangularMath.GetPeakNeighbour(Current, _horizontalNeighbourIndexPeak) 
                    : TriangularMath.GetValleyNeighbour(Current, _horizontalNeighbourIndexValley);

                Current = nextPos;
                return true;
            }
        }

        public void Reset() { }
        public void Dispose() { }

        IEnumerator IEnumerable.GetEnumerator() => this;
        public IEnumerator<IntTriangularPos> GetEnumerator() => this;       
    }
}
