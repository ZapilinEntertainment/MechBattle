using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Pathfinding;

namespace ZE.MechBattle.Ecs.Pathfinding {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class PathRequestsHandleSystem : ISystem 
    {
        private readonly struct RequestedPath
        {
            public readonly float3 Start;
            public readonly float3 End;
            public readonly Entity Entity;

            public RequestedPath(Entity entity, float3 start, float3 end)
            {
                Entity = entity;
                Start = start;
                End = end;
            }
        }

        public World World { get; set;}
        private Filter _pathRequestsFilter;
        private Stash<PositionComponent> _positions;
        private Stash<MoveTargetComponent> _targets;
        private Stash<PathTokenComponent> _pathTokens;

        private readonly PathsManager _pathsManager;

        [Inject]
        public PathRequestsHandleSystem(PathsManager pathsManager)
        {
            _pathsManager = pathsManager;
        }

        public void OnAwake() 
        {
            _pathRequestsFilter = World.Filter
                .With<MoveTargetComponent>()
                .With<PathfindingUserTag>()
                .Without<PathTokenComponent>()
                .Without<PathEndReachedTag>()
                .Build();

            _positions = World.GetStash<PositionComponent>();
            _targets = World.GetStash<MoveTargetComponent>();
            _pathTokens = World.GetStash<PathTokenComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_pathRequestsFilter.IsEmpty())
                return;

            foreach (var entity in _pathRequestsFilter)
            {
                var position = _positions.Get(entity).Value;
                var target = _targets.Get(entity).Value;

                UnityEngine.Debug.Log(position);
                var path = ABPath.Construct(position, target);
                AstarPath.StartPath(path);
                var pathToken = _pathsManager.Register(new(path, this));
                _pathTokens.Set(entity, new() { Value = pathToken, PathEnd = target});
            }
        }

        public void Dispose() { }
    }
}