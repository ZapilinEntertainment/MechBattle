using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexPath
    {
        public readonly HexPathNodeKey[] Points;
        public readonly float Cost;
        public HexPathNodeKey Start => Points[0];
        public HexPathNodeKey End => Points[Points.Length - 1];
        public HexPathKey GetKey() => new(Start,End);

        public HexPath(HexPathNodeKey[] pts, float cost) 
        {
            Points = pts;
            Cost = cost;
        }
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
