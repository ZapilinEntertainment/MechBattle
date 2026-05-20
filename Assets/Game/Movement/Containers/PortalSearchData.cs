using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct PortalSearchData
    {
        public readonly int2 HexCoord;
        public readonly IntTriangularPos Tripos;
        public readonly int ZoneIndex;

        public PortalSearchData(int2 hexCoord, IntTriangularPos tripos, int zoneIndex)
        {
            HexCoord = hexCoord;
            Tripos = tripos;
            ZoneIndex = zoneIndex;
        }
    }
}
