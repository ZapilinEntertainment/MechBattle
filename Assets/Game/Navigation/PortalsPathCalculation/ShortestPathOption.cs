namespace ZE.MechBattle.Navigation.PortalPathCalculation
{
    internal readonly struct ShortestPathOption
    {
        public readonly int PortalId;
        public readonly float Length;
        private const int INVALID_PATH_ID = -1;

        public bool IsValid => PortalId != INVALID_PATH_ID;

        private ShortestPathOption(int pathId, float length)
        {
            PortalId = pathId;
            Length = length;
        }

        public ShortestPathOption TryUpdate(int otherPathId, float otherPathLength)
        {
            if (otherPathLength < Length)
                return new(otherPathId, otherPathLength);
            else
                return this;
        }

        public static ShortestPathOption Default = new(pathId: INVALID_PATH_ID, length: float.MaxValue);
    }
}
