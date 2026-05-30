using System;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{
    public readonly struct PathProcessLaunchData<T>  where T : unmanaged
    {
        public readonly T Start;
        public readonly T End;

        public PathProcessLaunchData(T start, T end)
        {
            Start = start;
            End = end;
        }
    }

    public abstract class PathCalculationProcessesManager<DestinationKey, NodeKey> : ProcessManagerBase<
        PathCalculationProcess<DestinationKey, NodeKey>,
        PathProcessLaunchData<DestinationKey>,
        PathCalculationProcessToken> 
        where NodeKey : unmanaged
        where DestinationKey: unmanaged
    {
        protected readonly IPathsList<DestinationKey, NodeKey> PathsList;   

        public PathCalculationProcessesManager(int maxProcessesCount, IPathsList<DestinationKey, NodeKey> pathsList) : base(maxProcessesCount) 
        {
            PathsList = pathsList;
        }

        protected override void HandleResults(PathCalculationProcess<DestinationKey, NodeKey> process)
        {
            var results = process.StopAndGetResults();
            PathsList.AddCalculatedPath(process.PathId, results);
        }

        protected override PathCalculationProcessToken LaunchProcess(PathProcessLaunchData<DestinationKey> launchData, PathCalculationProcess<DestinationKey, NodeKey> process, int index)
        {
            var reservedPathId = PathsList.ReservePath(launchData.Start, launchData.End);
            process.Launch(new(reservedPathId, launchData.Start, launchData.End));
            //UnityEngine.Debug.Log($"start calculation: {start} -> {end}");
            return new(reservedPathId, index, process.ProcessIteration);
        }
    }
}
