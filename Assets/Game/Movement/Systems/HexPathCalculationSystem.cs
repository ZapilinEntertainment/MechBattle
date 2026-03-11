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
        private readonly NavigationMap _map;  
        private const int MAX_CALCULATIONS_PER_FRAME = 8;

        [Inject]
        public HexPathCalculationSystem(NavigationPathsList list, NavigationMap map)
        {
            _pathsList = list;
            _map = map;
        }

        public void OnAwake() 
        {
            
           
        }

        public void OnUpdate(float deltaTime) 
        {
            if (!_pathsList.TryGetRequestedPaths(MAX_CALCULATIONS_PER_FRAME, out var paths))
                return;

            foreach (var path in paths)
            {
                var job = new ConstructHexPathJob()
                {

                };
            }
        }

        public void Dispose()
        {

        }
    }
}