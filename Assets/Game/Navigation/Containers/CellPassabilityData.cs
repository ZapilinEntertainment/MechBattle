using System.Runtime.CompilerServices;


namespace ZE.MechBattle.Navigation
{
    public readonly struct CellPassabilityData
    {
        public readonly bool IsPassable;
        public readonly int NeighboursMask;
        public readonly int ZoneIndex;

        public CellPassabilityData(bool isPassable, int neighboursMask, int zoneIndex)
        {
            IsPassable = isPassable;
            NeighboursMask = neighboursMask;
            ZoneIndex = zoneIndex;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNeighbourAccessible(int neighbourIndex)
        {
            if ((uint)neighbourIndex >= 12) return false;
            return (NeighboursMask & (1 << neighbourIndex)) != 0;
        }
    }

}
