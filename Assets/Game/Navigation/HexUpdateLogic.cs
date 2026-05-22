using Unity.Collections;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class HexUpdateLogic
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

                UnityEngine.Debug.Log($"{hexPathNodeKey} -> {refinedResults[i]}");
                startHexCoord = refinedResults[i].ToNextHexCoord();
            }
            return refinedResults;
        }

        public static void ApplyPreparedCellDataOntoMap(PrepareNavCellDataProcess process, IUpdatableMap map)
        {
            var index = 0;
            foreach (var tripos in new HexTrianglesEnumerator(process.CurrentHexCenter, map.TrianglesPerHexEdge))
            {
                var navCell = map.GetNavigationCell(tripos);
                navCell.HeightData = process.GetHeightData(index);
                navCell.Passability = process.GetPassabilityData(index);
                map.UpdateNavigationCell(tripos, navCell);
                index++;
            }
        }
    
    }
}
