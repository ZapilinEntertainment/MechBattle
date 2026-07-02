using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ProjectileCreateSystem : ISystem 
    {
        public World World { get; set;}
        private Stash<ProjectileBuildRequest> _requests;
        private Filter _requestsFilter;
        private readonly ProjectilesFactory _factory;

        [Inject]
        public ProjectileCreateSystem(ProjectilesFactory builder)
        {
            _factory = builder;
        }

        public void OnAwake() 
        {
            _requestsFilter = World.Filter.With<ProjectileBuildRequest>().Build();
            _requests = World.GetStash<ProjectileBuildRequest>();
        }

        public void OnUpdate(float deltaTime) 
        { 
            if (_requestsFilter.IsNotEmpty())
            {
                foreach (var request in _requestsFilter)
                {
                    var data = _requests.Get(request);
                    _factory.Build(data.IdKey, data.Point, data.Shooter);
                    World.RemoveEntity(request);
                }
            }
        }

        public void Dispose() { }
    }
}