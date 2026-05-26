using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct PortalPathDestinationKey
    {
        public readonly int2 HexCoord;
        public readonly int ZoneIndex;

        public PortalPathDestinationKey(int2 hexCoord, int zoneIndex)
        {
            HexCoord = hexCoord;
            ZoneIndex = zoneIndex;
        }
    }
}
