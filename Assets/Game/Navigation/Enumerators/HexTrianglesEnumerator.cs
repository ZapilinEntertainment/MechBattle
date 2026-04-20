using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using System.Collections;

namespace ZE.MechBattle.Navigation
{
    // todo: make same order, as in flattened hex
    [BurstCompile]
    public struct HexTrianglesEnumerator : IEnumerator<IntTriangularPos>, IEnumerable<IntTriangularPos>
    {
        public IntTriangularPos Current => _sectorEnumerator.Current;
        private readonly int _radius;
        private readonly int _trisPerSector;
        private readonly IntTriangularPos _center;

        private int _sectorIndex;
        private SubtrianglesCoordsEnumerator _sectorEnumerator;
        

        public HexTrianglesEnumerator(IntTriangularPos center, int radius)
        {
            _center = center;
            _radius = radius;
            _trisPerSector = _radius * _radius;

            _sectorIndex = default;
            _sectorEnumerator = default;
            ChangeSector(0);
        }

        public bool MoveNext()
        {
            var canContinue = _sectorEnumerator.MoveNext();
            if (canContinue)
            {
                return true;
            }
            else
            {
                if (_sectorIndex < 5)
                {
                    ChangeSector(_sectorIndex + 1);
                    return _sectorEnumerator.MoveNext();
                }                
            }

            return false;
        }

        private void ChangeSector(int sectorIndex)
        {
            _sectorIndex = sectorIndex;

            var edge = (HexEdge)_sectorIndex;
            var sector = (HexSector)_sectorIndex;
            var innerRingTriangle = _center + edge.ToTriangleOffsetVector();            
            var pinnacle = sector.GetPinnaclePos(innerRingTriangle, _radius);

            _sectorEnumerator = new(pinnacle, _radius);
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }

        public void Reset() => ChangeSector(0);

        public HexTrianglesEnumerator GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;

        IEnumerator<IntTriangularPos> IEnumerable<IntTriangularPos>.GetEnumerator() => this;
    }


}
