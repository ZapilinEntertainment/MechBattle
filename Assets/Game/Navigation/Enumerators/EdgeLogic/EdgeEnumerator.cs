using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public interface IEdgeEnumerationLogic
    {
        bool StartsWithPeak { get; }
        IntTriangularPos GetStart(int trianglesPerEdge, NavigationHexPosition hex);
        IntTriangularPos EvenStep(IntTriangularPos pos);
        IntTriangularPos OddStep(IntTriangularPos pos);
    }

    [BurstCompile]
    public struct EdgeEnumerator<T> : IEdgeTrisEnumerator where T : struct, IEdgeEnumerationLogic
    {
        private T _logic;
        private bool _isEvenStep;
        private IntTriangularPos _pos;

        private int _triangleIndex;
        private int _totalTrisCount;

        public EdgeEnumerator(int trianglesPerEdge, NavigationHexPosition hex)
        {
            _logic = default; 
            _pos = _logic.GetStart(trianglesPerEdge, hex);
            _isEvenStep = true;

            _triangleIndex = 0;
            _totalTrisCount = TriangularMath.GetTwoRowEdgeTrianglesCount(trianglesPerEdge);
        }

        public EdgeEnumerator(NavigationPortalExit exit)
        {
            _logic = default;
            _pos = exit.StartTriangle;

            _isEvenStep = _logic.StartsWithPeak == _pos.IsPeak;      
            
            _triangleIndex = 0;
            _totalTrisCount = exit.Length;
        }

        public bool MoveNext()
        {
            if (_triangleIndex == 0)
            {
                _triangleIndex++;
                return true;
            }

            if (_triangleIndex < _totalTrisCount)
            {
                if (_isEvenStep)
                    _pos = _logic.EvenStep(_pos);
                else
                    _pos = _logic.OddStep(_pos);

                _isEvenStep = !_isEvenStep;
                _triangleIndex++;
                return true;

            }
            return false;
        }

        public IntTriangularPos Current => _pos;
        object IEnumerator.Current => Current;
        public EdgeEnumerator<T> GetEnumerator() => this;
        public void Dispose() { }
        public void Reset() { }

        IEnumerator<IntTriangularPos> IEnumerable<IntTriangularPos>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


    public struct TopEdgeEnumerationLogic : IEdgeEnumerationLogic
    {
        // Top left corner -> top right
        public bool StartsWithPeak => false;
        public const PeakNeighbour PeakDirection = PeakNeighbour.EdgeUpRight;
        public const ValleyNeighbour ValleyDirection = ValleyNeighbour.EdgeDownRight;

        public IntTriangularPos GetStart(int trianglesPerEdge, NavigationHexPosition hex)
        {
            return hex.TriangularCenterPos + new int3(0, trianglesPerEdge,- trianglesPerEdge + 1);            
        }
        public IntTriangularPos EvenStep(IntTriangularPos pos) => TriangularMath.GetValleyNeighbour(pos, ValleyDirection);
        public IntTriangularPos OddStep(IntTriangularPos pos) => TriangularMath.GetPeakNeighbour(pos, PeakDirection);

        public static IntTriangularPos AlongsidePeakVector => PeakDirection.ToTriangularOffsetVector();
        public static IntTriangularPos AlongsideValleyVector => ValleyDirection.ToTriangularOffsetVector();
    }

    public struct TopRightEdgeEnumerationLogic : IEdgeEnumerationLogic
    {
        // Top right corner -> right corner
        public bool StartsWithPeak => true;
        public const PeakNeighbour PeakDirection = PeakNeighbour.EdgeDown;
        public const ValleyNeighbour ValleyDirection = ValleyNeighbour.EdgeDownRight;

        public IntTriangularPos GetStart(int trianglesPerEdge, NavigationHexPosition hex)
        {
            return hex.TriangularCenterPos + new int3(-trianglesPerEdge, trianglesPerEdge - 1, 0);
        }
        public IntTriangularPos OddStep(IntTriangularPos pos) => TriangularMath.GetValleyNeighbour(pos, ValleyDirection);
        public IntTriangularPos EvenStep(IntTriangularPos pos) => TriangularMath.GetPeakNeighbour(pos, PeakDirection);

        public static IntTriangularPos AlongsidePeakVector => PeakDirection.ToTriangularOffsetVector();
        public static IntTriangularPos AlongsideValleyVector => ValleyDirection.ToTriangularOffsetVector();
    }

    public struct BottomRightEdgeEnumerationLogic : IEdgeEnumerationLogic
    {
        // right corner -> bottom right corner
        public bool StartsWithPeak => false;
        public const PeakNeighbour PeakDirection = PeakNeighbour.EdgeDown;
        public const ValleyNeighbour ValleyDirection = ValleyNeighbour.EdgeDownLeft;

        public IntTriangularPos GetStart(int trianglesPerEdge, NavigationHexPosition hex)
        {
            return hex.TriangularCenterPos + new int3(-trianglesPerEdge + 1, 0, trianglesPerEdge);
        }
        public IntTriangularPos OddStep(IntTriangularPos pos) => TriangularMath.GetPeakNeighbour(pos, PeakDirection);
        public IntTriangularPos EvenStep(IntTriangularPos pos) => TriangularMath.GetValleyNeighbour(pos, ValleyDirection);

        public static IntTriangularPos AlongsidePeakVector => PeakDirection.ToTriangularOffsetVector();
        public static IntTriangularPos AlongsideValleyVector => ValleyDirection.ToTriangularOffsetVector();
    }

    public struct BottomEdgeEnumerationLogic : IEdgeEnumerationLogic
    {
        // bottom right corner -> bottom left corner
        public bool StartsWithPeak => true;
        public const PeakNeighbour PeakDirection = PeakNeighbour.EdgeUpLeft;
        public const ValleyNeighbour ValleyDirection = ValleyNeighbour.EdgeDownLeft;

        public IntTriangularPos GetStart(int trianglesPerEdge, NavigationHexPosition hex)
        {
            return hex.TriangularCenterPos + new int3(0, -trianglesPerEdge, trianglesPerEdge - 1);
        }
        public IntTriangularPos EvenStep(IntTriangularPos pos) => TriangularMath.GetPeakNeighbour(pos, PeakDirection);
        public IntTriangularPos OddStep(IntTriangularPos pos) => TriangularMath.GetValleyNeighbour(pos, ValleyDirection);

        public static IntTriangularPos AlongsidePeakVector => PeakDirection.ToTriangularOffsetVector();
        public static IntTriangularPos AlongsideValleyVector => ValleyDirection.ToTriangularOffsetVector();
    }

    public struct BottomLeftEdgeEnumerationLogic : IEdgeEnumerationLogic
    {
        // bottom left corner -> left corner
        public bool StartsWithPeak => false;
        public const PeakNeighbour PeakDirection = PeakNeighbour.EdgeUpLeft;
        public const ValleyNeighbour ValleyDirection = ValleyNeighbour.EdgeUp;

        public IntTriangularPos GetStart(int trianglesPerEdge, NavigationHexPosition hex)
        {
            return hex.TriangularCenterPos + new int3(trianglesPerEdge, -trianglesPerEdge + 1, 0);
        }
        public IntTriangularPos OddStep(IntTriangularPos pos) => TriangularMath.GetPeakNeighbour(pos, PeakDirection);
        public IntTriangularPos EvenStep(IntTriangularPos pos) => TriangularMath.GetValleyNeighbour(pos, ValleyDirection);

        public static IntTriangularPos AlongsidePeakVector => PeakDirection.ToTriangularOffsetVector();
        public static IntTriangularPos AlongsideValleyVector => ValleyDirection.ToTriangularOffsetVector();
    }

    public struct TopLeftEdgeEnumerationLogic : IEdgeEnumerationLogic
    {
        // left corner -> top left corner
        public bool StartsWithPeak => true;
        public const PeakNeighbour PeakDirection = PeakNeighbour.EdgeUpRight;
        public const ValleyNeighbour ValleyDirection = ValleyNeighbour.EdgeUp;

        public IntTriangularPos GetStart(int trianglesPerEdge, NavigationHexPosition hex)
        {
            return hex.TriangularCenterPos + new int3(trianglesPerEdge-1, 0, -trianglesPerEdge);
        }
        public IntTriangularPos EvenStep(IntTriangularPos pos) => TriangularMath.GetPeakNeighbour(pos, PeakNeighbour.EdgeUpRight);
        public IntTriangularPos OddStep(IntTriangularPos pos) => TriangularMath.GetValleyNeighbour(pos, ValleyNeighbour.EdgeUp);

        public static IntTriangularPos AlongsidePeakVector => PeakDirection.ToTriangularOffsetVector();
        public static IntTriangularPos AlongsideValleyVector => ValleyDirection.ToTriangularOffsetVector();
    }
}
