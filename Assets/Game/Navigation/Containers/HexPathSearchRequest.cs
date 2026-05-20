using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public readonly struct HexPathSearchRequest
    {
        public readonly int2 StartHexCoord;
        public readonly int2 EndHexCoord;
        public readonly IntTriangularPos StartTripos;
        public readonly IntTriangularPos EndTripos;
        public readonly int StartHexZoneIndex;
        public readonly int EndHexZoneIndex;

        public HexPathSearchRequest(
            IntTriangularPos startTripos, 
            IntTriangularPos endTripos, 
            int2 startHexCoord, 
            int2 endHexCoord,
            int startHexZoneIndex,
            int endHexZoneIndex)
        {
            StartHexCoord = startHexCoord;
            EndHexCoord = endHexCoord;
            StartTripos = startTripos;
            EndTripos = endTripos;
            StartHexZoneIndex = startHexZoneIndex;
            EndHexZoneIndex = endHexZoneIndex;
        }
    
    }
}
