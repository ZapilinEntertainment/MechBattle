using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public static class ChangeTrianglePathJobSetupDataCommand
    {
        public static void Execute (
            ref ConstructTriangularPathJob job, 
            TriangularPathJobCollections collections,
            IntTriangularPos anyHexTriangle,
            INavigationMap map)
        {
            var hexPos = new NavigationHexPosition(anyHexTriangle, map);
            collections.ChangeCenter(hexPos);

            var index = 0;
            var passabilityArray = collections.PassabilityData;
            var calculationArray = collections.CalculationData;
            foreach (var tripos in new HexTrianglesEnumerator(hexPos.TriangularCenterPos, map.TrianglesPerHexEdge))
            {
                passabilityArray[index] = map.GetPassabilityData(tripos);
                calculationArray[index] = new(tripos);
                index++;
            }
            job.PassabilityData = collections.PassabilityData;
        }
    
    }
}
