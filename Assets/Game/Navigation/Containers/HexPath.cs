using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class HexPath
    {
        public readonly HexPathNodeKey[] Points;
        public readonly float Cost;

        public HexPath(HexPathNodeKey[] pts, float cost) 
        {
            Points = pts;
            Cost = cost;
        }

        public bool TryGetNode(int index, out HexPathNodeKey node)
        {
            if (index < 0 || index > Points.Length)
            {
                node = default;
                return false;
            }
            
            node = Points[index];
            return true;
        }
    }
}
