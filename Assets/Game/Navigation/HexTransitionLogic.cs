using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class HexTransitionLogic
    {

        public static float6 GetDirectionCostCoefficients(int2 startHexCoord, int2 endHexCoord)
        {
            var cf = new float6();
            var dir = math.normalize(endHexCoord - startHexCoord);
            for (var i = 0; i < 6; i++)
            {
                var edge = (HexEdge)i;
                cf[edge] = 1f + 0.2f * math.dot(dir, math.normalize(edge.ToHexOffsetVector()));
            }
            return cf;
        }
    }
}
