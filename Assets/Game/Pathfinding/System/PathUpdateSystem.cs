using VContainer;
using Unity.Mathematics;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs.Pathfinding 
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PathUpdateSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _awaitingPathsFilter;
        private Filter _updatingPathsFilter;
        private Stash<PathTokenComponent> _tokens;
        private Stash<PathProgressComponent> _pathProgress;
        private Stash<PositionComponent> _positions;
        private Stash<PathEndReachedTag> _pathEndReachedTag;
        private readonly PathsManager _pathsManager;

        [Inject]
        public PathUpdateSystem(PathsManager pathsManager)
        {
            _pathsManager = pathsManager;
        }

        public void OnAwake() 
        {
            _awaitingPathsFilter = World.Filter
                .With<PathTokenComponent>()
                .Without<PathProgressComponent>()
                .Without<PathEndReachedTag>()
                .Build();  
            _updatingPathsFilter = World.Filter
                .With<PathTokenComponent>()
                .With<PathProgressComponent>()
                .Without<PathEndReachedTag>()
                .Build();

            _tokens = World.GetStash<PathTokenComponent>();
            _pathProgress = World.GetStash<PathProgressComponent>();
            _positions = World.GetStash<PositionComponent>();
            _pathEndReachedTag = World.GetStash<PathEndReachedTag>();
        }


        public void OnUpdate(float deltaTime) 
        {
            if (_awaitingPathsFilter.IsNotEmpty())
            {
                foreach (var entity in _awaitingPathsFilter)
                {
                    if (_pathsManager.TryGetPath(_tokens.Get(entity).Value, out var path))
                    {
                        //UnityEngine.Debug.Log($"path ready: {path.startPoint} -> {path.endPoint}");

                        _pathProgress.Set(entity, new() { 
                            NextPosition = path.vectorPath[0], 
                            PathNodeIndex = 0, 
                            PathLength = path.vectorPath.Count });
                    }
                }
            }

            if (_updatingPathsFilter.IsNotEmpty())
            {
                foreach (var entity in _updatingPathsFilter)
                {
                    ref var progressComponent = ref _pathProgress.Get(entity);
                    var nextDist = progressComponent.NextPosition;
                    var currentPos = _positions.Get(entity).Value;

                    if (math.distancesq(currentPos, nextDist) <= math.EPSILON)
                    {
                        progressComponent.PathNodeIndex++;
                        var token = _tokens.Get(entity).Value;
                        if (progressComponent.PathNodeIndex == progressComponent.PathLength 
                            || !_pathsManager.TryGetPath(token, out var path))
                        {
                            _pathEndReachedTag.Add(entity);
                            _pathProgress.Remove(entity);
                            _pathsManager.Unregister(token);
                        }
                        else
                        {
                            progressComponent.NextPosition = path.vectorPath[progressComponent.PathNodeIndex];
                        }
                    }                    
                }
            }
            
        }

        public void Dispose()
        {

        }
    }
}