using Unity.Mathematics;
using Unity.Burst;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    [BurstCompile]
    public struct TriangleRadiusEnumerator
    {
        private readonly IntTriangularPos _center;
        private readonly int _radius;
        private int _currentIndex;
        private int _nextRingIndex;
        private int _ringIndex;

        public TriangleRadiusEnumerator(IntTriangularPos center, int radiusInTriangleHeights)
        {
            _center = center;
            _radius = radiusInTriangleHeights;
            
            Current = _center;
            _currentIndex = 0;
            _ringIndex = 0;

            _nextRingIndex = 1;
        }

        public IntTriangularPos Current { get; private set; }

        public bool MoveNext()
        {
            _currentIndex++;
            if (_currentIndex == _nextRingIndex)
            {
                _ringIndex++;
                if (_ringIndex % 2 == 0)
                {
                    // far neighbours (vertex)
                    _nextRingIndex = _currentIndex + 13;
                }
                else
                {
                    // close neighbours (edge)
                    _nextRingIndex = _currentIndex + 4;
                }

            }




            return false;
        }

        public TriangleRadiusEnumerator GetEnumerator() => this;
    }
}
