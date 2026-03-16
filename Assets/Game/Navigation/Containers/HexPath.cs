using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexPath
    {
        public readonly HexPathNodeKey[] Points;
        public HexPathNodeKey Start => Points[0];
        public HexPathNodeKey End => Points[Points.Length - 1];
        public HexPathKey GetKey() => new(Start,End);

        public HexPath(HexPathNodeKey[] pts) => Points = pts;
    }

    public readonly struct HexPathKey
    {
        public readonly HexPathNodeKey Start;
        public readonly HexPathNodeKey End;

        public HexPathKey(HexPathNodeKey start, HexPathNodeKey end)
        {
            Start = start;
            End = end;
        }
    }
}
