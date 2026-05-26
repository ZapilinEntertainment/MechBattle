using ZE.Utils;

namespace ZE.MechBattle
{

    public readonly struct PathInput<DestinationKey> where DestinationKey : unmanaged
    {
        public readonly int PathId;
        public readonly DestinationKey Start;
        public readonly DestinationKey End;

        public PathInput(int pathId, DestinationKey start, DestinationKey end)
        {
            PathId = pathId;
            Start = start;
            End = end;
        }
    }

    public abstract class PathCalculationProcess<DestinationKey, NodeKey> : JobProcessBase<PathInput<DestinationKey>, PathCalculationResult<DestinationKey, NodeKey>> 
        where NodeKey : unmanaged
        where DestinationKey : unmanaged
    {
        public int PathId { get; private set; }

        public override void Launch(PathInput<DestinationKey> input)
        {
            PathId = input.PathId;
            base.Launch(input);
        }
    }
}
