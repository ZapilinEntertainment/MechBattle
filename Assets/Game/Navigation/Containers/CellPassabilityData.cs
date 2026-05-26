using Unity.Burst;
using System.Runtime.CompilerServices;


namespace ZE.MechBattle.Navigation
{
    public readonly struct CellPassabilityData
    {
        public readonly bool IsPassable;
        public readonly int NeighboursMask;
        public readonly int ZoneIndex;
        public readonly float EntranceCost;

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

        public CellPassabilityData ChangeZone(int newZoneIndex) => new(IsPassable, NeighboursMask, newZoneIndex, EntranceCost);

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
