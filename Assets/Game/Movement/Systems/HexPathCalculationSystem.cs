using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathCalculationSystem : ISystem 
    {
        public World World { get; set;}
        private readonly NavigationPathsList _pathsList;
        private readonly NativeArray<int2> _offsets = new NativeArray<int2>(
            new int2[]
            {
                new int2(0, -1),
                new int2(1, -1),
                new int2(1, 0),
                new int2(0, 1),
                new int2(-1, 1),
                new int2(-1, 0) 
            },
            Allocator.Persistent);
        private const int MAX_CALCULATIONS_PER_FRAME = 8;

        [Inject]
        public HexPathCalculationSystem(NavigationPathsList list)
        {
            _pathsList = list;
        }

        public void OnAwake() 
        {
            
        }

        public void OnUpdate(float deltaTime) 
        {
            if (!_pathsList.TryGetRequestedPaths(MAX_CALCULATIONS_PER_FRAME, out var paths))
                return;

            // todo: need to calculate hexes in navigation map
            // and add them

            return;
            foreach (var path in paths)
            {
                var job = new ConstructHexPathJob()
                {
                   // NeighborOffsets = _offsets,
                   // Width = 
                };
            }
        }

        public void Dispose()
        {
            _offsets.Dispose();
        }
    }
}