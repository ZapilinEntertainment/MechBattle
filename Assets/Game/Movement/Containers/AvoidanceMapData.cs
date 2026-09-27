namespace ZE.MechBattle
{
    public readonly struct AvoidanceMapData
    {
        public readonly bool IsPassable;
        public readonly float MovementDensity;    

        public AvoidanceMapData(bool isPassable, float movementDensity)
        {
            IsPassable = isPassable;
            MovementDensity = movementDensity;
        }
    }
}
