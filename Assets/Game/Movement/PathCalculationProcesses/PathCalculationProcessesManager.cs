using System;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public readonly struct PathProcessLaunchData<NodeKey> : IProcessLaunchData where NodeKey : unmanaged
    {
        public readonly NodeKey Start;
        public readonly NodeKey End;

        public PathProcessLaunchData(NodeKey start, NodeKey end)
        {
            Start = start;
            End = end;
        }
    }

    public abstract class PathCalculationProcessesManager<NodeKey> : ProcessManagerBase<
        PathInput<NodeKey>, 
        PathCalculationResult<NodeKey>,
        PathCalculationProcess<NodeKey>,
        PathProcessLaunchData<NodeKey>,
        PathCalculationProcessToken> 
        where NodeKey : unmanaged
    {
        protected readonly IPathsList<NodeKey> PathsList;   

        public PathCalculationProcessesManager(int maxProcessesCount, IPathsList<NodeKey> pathsList) : base(maxProcessesCount) 
        {
            PathsList = pathsList;
        }

        protected override void HandleResults(PathCalculationProcess<NodeKey> process)
        {
            var results = process.StopAndGetResults();
            PathsList.AddCalculatedPath(process.PathId, results);
        }

        protected override PathCalculationProcessToken LaunchProcess(PathProcessLaunchData<NodeKey> launchData, PathCalculationProcess<NodeKey> process, int index)
        {
            var reservedPath = PathsList.ReservePath((launchData.Start, launchData.End));
            process.Launch(new(reservedPath.Id, launchData.Start, launchData.End));
            //UnityEngine.Debug.Log($"start calculation: {start} -> {end}");
            return new(reservedPath.Id, index, process.ProcessIteration);
        }
    }
}
