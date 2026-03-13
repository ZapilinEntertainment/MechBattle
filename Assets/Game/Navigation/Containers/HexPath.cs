using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexPath
    {
        public readonly int2[] Points;
        public int2 Start => Points[0];
        public int2 End => Points[Points.Length - 1];
        public int4 EdgePoints => new(Start, End);

        public HexPath(int2[] pts) => Points = pts;
    }
}
