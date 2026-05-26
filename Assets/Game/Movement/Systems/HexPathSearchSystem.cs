using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class HexPathSearchSystem : ISystem 
    {
        public World World { get; set;}
        private readonly HexPortalPathsLRUBuffer _hexPaths;
        private Filter _filter;
        private Stash<HexPathSearchRequestComponent> _searchRequests;
        private Stash<HexPathCalculationRequestTag> _calculationTags;
        private Stash<HexPathComponent> _hexPathComponents;

        [Inject]
        public HexPathSearchSystem(HexPortalPathsLRUBuffer hexPaths)
        {
            _hexPaths = hexPaths;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<HexPathSearchRequestComponent>().Build();

            _searchRequests = World.GetStash<HexPathSearchRequestComponent>();
            _hexPathComponents = World.GetStash<HexPathComponent>();
            _calculationTags = World.GetStash<HexPathCalculationRequestTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var requestComponent = _searchRequests.Get(entity);
                var start = requestComponent.Start;
                var end = requestComponent.End;
                int pathId;
                if (_hexPaths.TryGetPathByEndpoints(start, end, out var path, updateUsingTime: true))
                {
                    pathId = path.Id;
                }
                else
                {
                    pathId = _hexPaths.ReservePath(start, end).Id;                    
                }

                _calculationTags.Add(entity);
                _hexPathComponents.Set(entity, new(pathId));
                _searchRequests.Remove(entity);
            }
        }

        public void Dispose()
        {

        }
    }
}