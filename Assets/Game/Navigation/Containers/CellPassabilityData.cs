using Unity.Burst;
using System.Runtime.CompilerServices;


namespace ZE.MechBattle.Navigation
{
    public struct CellPassabilityData
    {
        public bool IsPassable;
        public int NeighboursMask;
        public int ZoneIndex;
        public float EntranceCost;

        public CellPassabilityData(
            bool isPassable, 
            int neighboursMask,
            int zoneIndex = NavigationConstants.DEFAULT_CELL_ZONE, 
            float entranceCost = NavigationConstants.DEFAULT_TRIANGLE_ENTRANCE_COST)
        {
            IsPassable = isPassable;
            NeighboursMask = neighboursMask;
            ZoneIndex = zoneIndex;
            EntranceCost = entranceCost;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNeighbourAccessible(int neighbourIndex) => IsNeighbourAccessible(neighbourIndex, NeighboursMask);

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNeighbourAccessible(int neighbourIndex, int mask)
        {
            if ((uint)neighbourIndex >= 12) return false;
            return (mask & (1 << neighbourIndex)) != 0;
        }
    }

}
