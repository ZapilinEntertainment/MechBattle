using Unity.Collections;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public static class HexPathLogic
    {
        public static NativeArray<HexPathNodeKey> RefineHexPath(int2 startHexCoord, NativeList<HexPathNodeKey> rawResults)
        {
            // note write into same array, no allocations
            var length = rawResults.Length;
            var refinedResults = rawResults.AsArray();
            for (var i = 0; i < length; i++)
            {
                var hexPathNodeKey = rawResults[i];
                if (math.any(hexPathNodeKey.HexCoord != startHexCoord))
                {
                    refinedResults[i] = hexPathNodeKey.ToOpposite();
                }
                else
                {
                    refinedResults[i] = hexPathNodeKey;
                }

                startHexCoord = refinedResults[i].ToOppositeHexCoord();
            }
            return refinedResults;
        }
    
    }
}
