using VContainer;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs.Pathfinding 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // check if nav path destination is not the sam as movement target
    public sealed class PathActualizingSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _unmatchedPathsFilter;
        private Filter _stoppedEntities;
        private Stash<MoveTargetComponent> _targets;
        private Stash<PathTokenComponent> _pathTokens;
        private Stash<PathEndReachedTag> _pathEndReachedTags;
        private readonly PathsManager _pathsManager;

        [Inject]
        public PathActualizingSystem(PathsManager pathsManager)
        {
            _pathsManager = pathsManager;
        }

        public void OnAwake() 
        {
            _unmatchedPathsFilter = World.Filter
                .With<MoveTargetComponent>()
                .With<PathTokenComponent>()
                .Build();

            _stoppedEntities = World.Filter
                .With<PathTokenComponent>()
                .Without<MoveTargetComponent>()
                .Build();

            _targets = World.GetStash<MoveTargetComponent>();
            _pathTokens = World.GetStash<PathTokenComponent>();
            _pathEndReachedTags = World.GetStash<PathEndReachedTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_unmatchedPathsFilter.IsNotEmpty())
            {
                foreach (var entity in _unmatchedPathsFilter)
                {
                    var pathTokenComponent = _pathTokens.Get(entity);
                    if (math.distancesq(pathTokenComponent.PathEnd, _targets.Get(entity).Value) > math.EPSILON)
                    {
                        _pathsManager.Unregister(pathTokenComponent.Value);
                        _pathTokens.Remove(entity);
                        _pathEndReachedTags.Remove(entity);
                    }
                }
            }
          
            if(_stoppedEntities.IsNotEmpty())
            {
                foreach (var entity in _stoppedEntities)
                {
                    _pathsManager.Unregister(_pathTokens.Get(entity).Value);
                    _pathTokens.Remove(entity);
                    _pathEndReachedTags.Remove(entity);
                }
            }
        }

        public void Dispose()
        {

        }
    }
}