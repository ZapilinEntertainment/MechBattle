using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public class MovementCollisionAvoidanceSystem : PausableSystem
    {
        private Filter _filter;
        private Stash<NextPositionComponent> _nextPositionComponents;
        private Stash<PositionComponent> _positionComponents;
        private Stash<TriangularPosComponent> _triangularPosComponents;
        private Stash<WaypointMoveTarget> _waypoints;

        private readonly IMovementCellsMap _movementCells;
        private readonly float _invertedTriangleHeight;
        private readonly float _triangleHeight;
        private readonly CollisionAvoidanceHandler _collisionAvoidanceHandler;

        [Inject]
        public MovementCollisionAvoidanceSystem(
            SceneFlagsManager flags, 
            IMovementCellsMap movementCells, 
            INavigationMap map,
            CollisionAvoidanceHandler collisionAvoidanceHandler) : base(flags)
        {
            _movementCells = movementCells;            
            _collisionAvoidanceHandler = collisionAvoidanceHandler;

            _invertedTriangleHeight = map.InvertedTriangleHeight;
            _triangleHeight = map.TriangleHeight;
        }

        public override void OnAwake()
        {
            _filter = World.Filter
                .With<NextPositionComponent>()
                .With<MovementCollisionAvoidanceComponent>()
                .Build();

            _nextPositionComponents = World.GetStash<NextPositionComponent>();
            _positionComponents = World.GetStash<PositionComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
            _waypoints = World.GetStash<WaypointMoveTarget>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;            

            foreach (var entity in _filter)
            {
                var nextPosComponent = _nextPositionComponents.Get(entity);

                // NOTE: target tripos may be in jump-triangle (vertex neighbourhood only), 
                // so we use waypoint target, not nextPosition.Tripos
                // it may cause some problems someday, need more complicated check logic here
                var nextTripos = _waypoints.Get(entity).TriangularPos;
                var currentTripos = _triangularPosComponents.Get(entity).Value;

                var currentWorldPos = _positionComponents.Get(entity).Value.xz;
                var moveDir = nextPosComponent.WorldPosXZ - currentWorldPos;

                if (!_movementCells.TryGetValue(nextTripos, out var moveCell))
                {
                    _movementCells.TryWriteCell(nextTripos, entity, moveDir, 1);
                    continue;
                }
                else
                {
                    if (moveCell.Entity == entity)
                    {
                        continue;
                    }
                    else
                    {
                        if (nextTripos == currentTripos)
                        {
                            GoThrough(entity, currentWorldPos, nextPosComponent.WorldPosXZ, 0.1f);
                            continue;
                        }
                        else
                        {
                            StandStill(entity);
                            continue;
                        }
                    }
                        
                }

                // if obstacling unit is standing
                if (math.all(moveCell.MoveVector == 0f))
                {
                    if (!TryDetour(entity, nextTripos, currentWorldPos, nextPosComponent.MoveDistance)) 
                    {
                        //if (_collisionAvoidanceHandler.CanEntitiesGoThrough(entity, moveCell.Entity))
                        //    GoThrough(entity, currentWorldPos, nextPosComponent.WorldPosXZ, 0.5f);
                        //else
                            StandStill(entity);
                    }
                    continue;
                }
                else
                {
                    StandStill(entity);
                    //if (_collisionAvoidanceHandler.CanEntitiesGoThrough(entity, moveCell.Entity))
                    //{
                    //    var dot = math.dot(moveCell.MoveVector, moveDir);
                    //    var speedCf = dot < 0f ? 0.25f : (dot * 0.5f + 0.5f);
                    //    GoThrough(entity, currentWorldPos, nextPosComponent.WorldPosXZ, speedCf);
                    //}
                    //else
                    //{
                    //    StandStill(entity);
                    //}
                }
            }
        }

        private bool TryDetour(
            Entity entity, 
            IntTriangularPos nextTripos, 
            float2 currentWorldPos, 
            float moveDistance)
        {
            var currentTripos = _triangularPosComponents.Get(entity).Value;
            var detourFound = _collisionAvoidanceHandler.TryGetDetour(currentTripos, nextTripos, out var detourTripos);

            if (detourFound)
            {
                var detourWorldPos = TriangularMath.TriangularToWorld(detourTripos, _triangleHeight).xz;
                var dir = math.normalizesafe(detourWorldPos - currentWorldPos);
                detourWorldPos = moveDistance * dir + currentWorldPos;
                _nextPositionComponents.Set(entity, new(detourWorldPos, detourTripos, moveDistance));

               // UnityEngine.Debug.Log($"detour found: {currentTripos} -> {detourTripos}");
            }
            else
            {
               // UnityEngine.Debug.Log($"cannot find detour on {currentTripos}");
            }

            return detourFound;
        }

        private void GoThrough(Entity entity, float2 currentWorldPos, float2 prevNextWorldPos, float speedCf)
        {
            var moveDir = speedCf * (prevNextWorldPos - currentWorldPos);
            var nextPos = currentWorldPos + moveDir;
            var nextTripos = TriangularMath.WorldToTrianglePosInvertedHeight(new float3(nextPos.x, 0f, nextPos.y), _invertedTriangleHeight);
            _nextPositionComponents.Set(entity, new(nextPos, nextTripos, math.length(moveDir)));
        }

        private void StandStill(Entity entity)
        {
            _nextPositionComponents.Set(entity, new(_positionComponents.Get(entity).Value.xz, _triangularPosComponents.Get(entity).Value, 0f));
            _waypoints.Remove(entity);
        }
    }
}
