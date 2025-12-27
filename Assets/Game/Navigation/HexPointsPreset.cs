using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexPointsPreset
    {
        public readonly float2 TopRight;
        public readonly float2 Right;
        public readonly float2 BottomRight;
        public readonly float2 BottomLeft;
        public readonly float2 Left;
        public readonly float2 TopLeft;

        public HexPointsPreset(float hexEdgeLength)
        {
            var axis = math.up();
            var fwd = TriangularMath.DirY;
            TopRight = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(30f)), fwd).xz;
            Right = new(hexEdgeLength, 0f);
            BottomRight = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(150f)), fwd).xz;

            axis = math.down();
            BottomLeft = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(150f)), fwd).xz;
            Left = new(-hexEdgeLength, 0f);
            TopLeft = hexEdgeLength * math.mul(quaternion.AxisAngle(axis, math.radians(30f)), fwd).xz;
        }
    
    }
}
