using ZE.MechBattle.Projectiles;

namespace ZE.MechBattle
{
    public readonly struct HighArcRaycastSegments
    {
        public readonly TrajectorySegment StartSegment;
        public readonly TrajectorySegment MiddleSegment;
        public readonly TrajectorySegment EndSegment;

        public const int SEGMENTS_COUNT = 3;

        public HighArcRaycastSegments(TrajectorySegment start, TrajectorySegment middle, TrajectorySegment end)
        {
            StartSegment = start;
            MiddleSegment = middle;
            EndSegment = end;
        }
    
    }
}
