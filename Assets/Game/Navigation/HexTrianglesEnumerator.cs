using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;
using System.Collections;

namespace ZE.MechBattle.Navigation
{
    [BurstCompile]
    public struct HexTrianglesEnumerator : IEnumerator<IntTriangularPos>, IEnumerable<IntTriangularPos>
    {
        public IntTriangularPos Current { get;private set;}
        private readonly int2 _hexCoord;
        private readonly int _radius;

        private int _circleIndex;
        private int _edgeIndex;
        private int _edgeStepIndex;

        private int _nextPeakDir;
        private int _nexValleyDir;

        public HexTrianglesEnumerator(NavigationHexPosition hexPos, int radius)
        {
            _hexCoord = hexPos.HexCoordinate;
            _radius = radius;
            Current = hexPos.TriangularCenterPos + new int3(0,1,0);

            _circleIndex = 0;
            _edgeIndex = 0;
            _edgeStepIndex = -1;

            _nextPeakDir = 0;
            _nexValleyDir = 0;
            SetupDirections(HexEdge.Top);
        }

        public bool MoveNext()
        {
            if (_edgeStepIndex == -1)
            {
                _edgeStepIndex = 0;
                return true;
            }

            _edgeStepIndex++;            
            var trianglesPerCurrentRingEdge = (_circleIndex+1) * 2 - 1;
            if (_edgeStepIndex == trianglesPerCurrentRingEdge)
            {
                _edgeStepIndex = 0;
                // change edge
                var prevEdge = (HexEdge)_edgeIndex;                
                switch (prevEdge)
                {
                    case HexEdge.Top: Current = TriangularMath.GetValleyNeighbour(Current, ValleyNeighbour.EdgeDownRight); break;
                    case HexEdge.TopRight: Current = TriangularMath.GetPeakNeighbour(Current, PeakNeighbour.EdgeDown); break;
                    case HexEdge.BottomRight: Current = TriangularMath.GetValleyNeighbour(Current, ValleyNeighbour.EdgeDownLeft); break;
                    case HexEdge.Bottom: Current = TriangularMath.GetPeakNeighbour(Current, PeakNeighbour.EdgeUpLeft); break;
                    case HexEdge.BottomLeft: Current = TriangularMath.GetValleyNeighbour(Current, ValleyNeighbour.EdgeUp); break;
                    case HexEdge.TopLeft: Current = TriangularMath.GetPeakNeighbour(Current, PeakNeighbour.EdgeUpRight); break;
                }      
                
                _edgeIndex++;
                if (_edgeIndex == 6)
                {
                    // going to outer circle
                    _edgeIndex = 0;
                    _circleIndex++;
                    if (_circleIndex == _radius)
                        return false;

                    Current = TriangularMath.GetValleyNeighbour(Current, ValleyNeighbour.VertexUpLeftValley);
                }

                SetupDirections((HexEdge)_edgeIndex);
            }
            else
            {
                Current = Current.IsPeak ? TriangularMath.GetPeakNeighbour(Current, _nextPeakDir) : TriangularMath.GetValleyNeighbour(Current, _nexValleyDir);
            }
            return true;
        }

        private void SetupDirections(HexEdge edge)
        {
            switch (edge)
            {
                case HexEdge.TopLeft:
                    {
                        _nextPeakDir = (int)PeakNeighbour.EdgeUpRight;
                        _nexValleyDir = (int)ValleyNeighbour.EdgeUp;
                        break;
                    }
                case HexEdge.BottomLeft:
                    {
                        _nextPeakDir = (int)PeakNeighbour.EdgeUpLeft;
                        _nexValleyDir = (int)ValleyNeighbour.EdgeUp;
                        break;
                    }
                case HexEdge.Bottom:
                    {
                        _nextPeakDir = (int)PeakNeighbour.EdgeUpLeft;
                        _nexValleyDir = (int)ValleyNeighbour.EdgeDownLeft;
                        break;
                    }
                case HexEdge.BottomRight:
                    {
                        _nextPeakDir = (int)PeakNeighbour.EdgeDown;
                        _nexValleyDir = (int)ValleyNeighbour.EdgeDownLeft;
                        break;
                    }
                case HexEdge.TopRight:
                    {
                        _nextPeakDir = (int)PeakNeighbour.EdgeDown;
                        _nexValleyDir = (int)ValleyNeighbour.EdgeDownRight;
                        break;
                    }
                default:
                    {
                        _nextPeakDir = (int)PeakNeighbour.EdgeUpRight;
                        _nexValleyDir = (int)ValleyNeighbour.EdgeDownRight;
                        break;
                    }
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() { }
        public void Reset() { }

        public IEnumerator<IntTriangularPos> GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }


}
