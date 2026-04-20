using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct TriangleEnumerationSettings
    {
        public readonly bool IsPeakZone;
        public readonly int SignCf;
        public readonly int ValleysIndexOffset;
        public readonly int3 StartPeakPos;
        public readonly int3 StartValleyPos;

        public IntTriangularPos GetPinnacle() => new(IsPeakZone ? StartPeakPos : StartValleyPos);

        public TriangleEnumerationSettings(IntTriangularPos pinnaclePos, int trianglesPerEdge)
        {
            IsPeakZone = pinnaclePos.IsPeak;
            SignCf = IsPeakZone ? -1 : 1;
            ValleysIndexOffset = TrianglesEnumerationLogic.CalculateValleyIndexOffset(trianglesPerEdge, IsPeakZone);

            StartPeakPos = IsPeakZone ? pinnaclePos : TriangularMath.GetValleyNeighbour(pinnaclePos, ValleyNeighbour.EdgeUp);
            StartValleyPos = IsPeakZone ? TriangularMath.GetPeakNeighbour(pinnaclePos, PeakNeighbour.EdgeDown) : pinnaclePos;

            //UnityEngine.Debug.Log($"pinnacle: {pinnaclePos} is peak zone: {IsPeakZone} start peak: {StartPeakPos} start valley: {StartValleyPos}");
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int3 GetStart(bool isPeak) => isPeak ? StartPeakPos : StartValleyPos;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntTriangularPos V2ToTriangular(int2 v2, bool isPeak)
        {
            var pos = new int3(-v2.x, v2.y, v2.x - v2.y) * SignCf;
            //UnityEngine.Debug.Log($"{index} -> {v2} -> {pos} with pinnacle peak at {_startPosPeak}");
            return new(pos + GetStart(isPeak));
        }


        // returns peak index or valley index from global
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetTypedIndex(int index, out bool isPeak)
        {
            isPeak = index < ValleysIndexOffset;
            return isPeak ? index : (index - ValleysIndexOffset);
        }
    }

    public static class TrianglesEnumerationLogic
    {       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateValleyIndexOffset(int trianglesPerEdge, bool isPeakZone)
        {
            var secondaryTypeTrianglesCount = trianglesPerEdge * (trianglesPerEdge - 1) / 2;
            return isPeakZone ? (trianglesPerEdge * trianglesPerEdge - secondaryTypeTrianglesCount) : (secondaryTypeTrianglesCount);
        }
    }
}
