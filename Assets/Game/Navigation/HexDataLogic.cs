using Unity.Collections;

namespace ZE.MechBattle.Navigation
{
    public static class HexDataLogic
    {
        public static void FulfilPeakDataArray(NativeBitArray peakData, IntTriangularPos hexCenter, int radius)
        {
            var i = 0;
            foreach (var pos in new HexTrianglesEnumerator(hexCenter, radius))
            {
                peakData.Set(i++, pos.IsPeak);
            }
        }
    
    }
}
