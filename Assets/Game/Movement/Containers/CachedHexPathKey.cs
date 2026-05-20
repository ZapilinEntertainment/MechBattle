using Unity.Mathematics;

namespace ZE.MechBattle
{
    public readonly struct CachedHexPathKey
    {
        public readonly int2 StartHexCoord;
        public readonly int2 EndHexCoord;
        public readonly int StartHexZoneIndex;
        public readonly int EndHexZoneIndex;
    
        public CachedHexPathKey(int2 startHexCoord, int2 endHexCoord, int startHexZoneIndex, int endHexZoneIndex)
        {
            StartHexCoord = startHexCoord;
            EndHexCoord = endHexCoord;
            StartHexZoneIndex = startHexZoneIndex;
            EndHexZoneIndex = endHexZoneIndex;
        }
    }
}
