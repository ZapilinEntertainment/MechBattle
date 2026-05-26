using ZE.Utils;

namespace ZE.MechBattle
{
    public readonly struct PathCalculationProcessToken : IProcessToken
    {
        public bool IsValid { get; }
        public readonly int PathId;
        public int ProcessIndex { get; }
        public int ProcessIteration { get; }

        public PathCalculationProcessToken(int pathId, int processIndex, int processIteration)
        {
            PathId = pathId;
            ProcessIndex = processIndex;
            ProcessIteration = processIteration;
            IsValid = true;
        }
    }
}
