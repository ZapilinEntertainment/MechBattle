namespace ZE.MechBattle
{
    public readonly struct PathCalculationProcessToken
    {
        public readonly bool IsValid;
        public readonly int PathId;
        public readonly int ProcessIndex;
        public readonly int ProcessIteration;

        public PathCalculationProcessToken(int pathId, int processIndex, int processIteration)
        {
            PathId = pathId;
            ProcessIndex = processIndex;
            ProcessIteration = processIteration;
            IsValid = true;
        }
    }
}
