using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WaypointsMovementSystem : PausableSystem 
    {
        private Filter _moveFilter;
        private Stash<PositionComponent> _positions;
        private Stash<WaypointMoveTarget> _waypoints;
        private Stash<MoveSpeedComponent> _moveSpeed;
        private Stash<NextPositionComponent> _nextPositionComponent;
        private readonly float _invertedTriangleHeight;

        public WaypointsMovementSystem(SceneFlagsManager flags, INavigationMap map) : base(flags) 
        { 
            _invertedTriangleHeight = map.InvertedTriangleHeight;
        }

        public override void OnAwake() 
        {
            _moveFilter = World.Filter
                .With<WaypointMoveTarget>()
                .With<MoveSpeedComponent>()
                .Build();

            _positions = World.GetStash<PositionComponent>();
            _waypoints = World.GetStash<WaypointMoveTarget>();
            _moveSpeed = World.GetStash<MoveSpeedComponent>();
            _nextPositionComponent = World.GetStash<NextPositionComponent>();
        }

        public override void OnUpdate(float deltaTime) 
        {
            if (IsPaused)
                return;

            foreach (var entity in _moveFilter)
            {
                var waypointPosition = _waypoints.Get(entity).WorldPos;
                var position = _positions.Get(entity).Value;
                var speed = _moveSpeed.Get(entity).Value;

                var endPos = MathExtensions.MoveTowards(position, waypointPosition, speed * deltaTime);
                _nextPositionComponent.Set(entity, new(endPos.xz, TriangularMath.WorldToTrianglePosInvertedHeight(endPos, _invertedTriangleHeight)));

                //UnityEngine.Debug.Log($"entity {entity.Id} WMS SET: {position} / {endPos}");
            }
        }
    }
}