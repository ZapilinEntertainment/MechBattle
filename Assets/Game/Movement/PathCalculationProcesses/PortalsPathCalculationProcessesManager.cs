using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class PortalsPathCalculationProcessesManager : PathCalculationProcessesManager<PortalSearchData>
    {
        public PortalsPathCalculationProcessesManager(int maxProcessesCount, IPathsList<int> pathsList) : base(maxProcessesCount, pathsList)
        {
        }

        public override PathCalculationProcess<PortalSearchData> CreateNewProcess() => new PortalsPathCalculationProcess();

        override protected PathCalculationProcessToken LaunchProcessJob(PortalSearchData start, PortalSearchData end, PathCalculationProcess<PortalSearchData> process, int index)
        {
            var reservedPath = PathsList.ReservePath((start, end));
            process.Launch(reservedPath.Id, start, end);
            //UnityEngine.Debug.Log($"start calculation: {start} -> {end}");
            return new(reservedPath.Id, index, process.ProcessIteration);
        }
    }
}
