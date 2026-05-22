using Unity.Collections;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class HexPathCalculationProcessManager : PathCalculationProcessesManager<HexPathNodeKey>
    {
        private readonly Allocator _allocator;
        private readonly INavigationMap _map;

        public HexPathCalculationProcessManager(Allocator allocator, INavigationMap map, int maxProcessesCount, IPathsList<HexPathNodeKey> pathsList) : base(maxProcessesCount, pathsList)
        {
            _allocator = allocator;
            _map = map;
        }

        protected override PathCalculationProcess<HexPathNodeKey> CreateNewProcess() => new HexPathCalculationProcess(_allocator, _map);
    }
}
