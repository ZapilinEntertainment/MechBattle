using ZE.MechBattle.Navigation;
using Unity.Collections;

namespace ZE.MechBattle
{
    public class TrianglePathCalculationProcessManager : PathCalculationProcessesManager<IntTriangularPos, IntTriangularPos>
    {
        private readonly INavigationMap _map;
        private readonly Allocator _allocator;

        public TrianglePathCalculationProcessManager(Allocator allocator, INavigationMap map, int maxProcessesCount, IPathsList<IntTriangularPos, IntTriangularPos> pathsList) : base(maxProcessesCount, pathsList)
        {
            _map = map;
            _allocator = allocator;
        }

        protected override PathCalculationProcess<IntTriangularPos, IntTriangularPos> CreateNewProcess() =>
            new TrianglePathCalculationProcess(_allocator, _map);
    }
}
