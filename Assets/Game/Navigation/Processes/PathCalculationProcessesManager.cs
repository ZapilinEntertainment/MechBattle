using System;
using ZE.Utils;

namespace ZE.MechBattle.Navigation
{

    public abstract class PathCalculationProcessesManager<DestinationKey, NodeKey> : ProcessManagerBase<
        PathCalculationProcess<DestinationKey, NodeKey>,
        PathInput<DestinationKey>,
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

        protected override PathCalculationProcessToken LaunchProcess(PathInput<DestinationKey> launchData, PathCalculationProcess<DestinationKey, NodeKey> process, int index)
        {
            process.Launch(launchData);
            //UnityEngine.Debug.Log($"start calculation: {start} -> {end}");
            return new(launchData.PathId, index, process.ProcessIteration);
        }
    }
}
