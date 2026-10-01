using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct AvoidanceMapData
    {
        public readonly bool IsPassable;
        public readonly int NeighboursAccessMask;
        public readonly float MovementDensity;    

        public AvoidanceMapData(CellPassabilityData passabilityData, float movementDensity )
        {
            IsPassable = passabilityData.IsPassable;
            MovementDensity = movementDensity;
            NeighboursAccessMask = passabilityData.NeighboursMask;
        }
    }
}
