using System.Runtime.CompilerServices;


namespace ZE.MechBattle.Navigation
{
    public struct CellPassabilityData
    {
        public bool IsPassable;
        public int NeighboursMask;
        public int ZoneIndex;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNeighbourAccessible(int neighbourIndex)
        {
            if ((uint)neighbourIndex >= 12) return false;
            return (NeighboursMask & (1 << neighbourIndex)) != 0;
        }
    }

}
