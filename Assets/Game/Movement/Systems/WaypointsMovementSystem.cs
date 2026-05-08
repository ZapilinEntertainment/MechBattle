using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WaypointsMovementSystem : PausableSystem 
    {
        private Filter _filter;
        private Stash<WaypointMoveTarget> _waypoints;
        private Stash<MoveSpeedComponent> _moveSpeed;
        private Stash<PositionComponent> _positions;

        public WaypointsMovementSystem(SceneFlagsManager flags) : base(flags) { }

        public override void OnAwake() 
        {
            _filter = World.Filter
                .With<WaypointMoveTarget>()
                .With<MoveSpeedComponent>()
                .Build();

            _waypoints = World.GetStash<WaypointMoveTarget>();
            _moveSpeed = World.GetStash<MoveSpeedComponent>();
            _positions = World.GetStash<PositionComponent>();
        }

        public override void OnUpdate(float deltaTime) 
        {
            if (IsPaused)
                return;

            foreach (var entity in _filter)
            {
                var waypointPosition = _waypoints.Get(entity).WorldPos;
                ref var positionsComponent = ref _positions.Get(entity);
                var speed = _moveSpeed.Get(entity).Value;

                var endPos = MathExtensions.MoveTowards(positionsComponent.Value, waypointPosition, speed * deltaTime);
                if (math.all(endPos == waypointPosition))
                    _waypoints.Remove(entity);

                positionsComponent.Value = endPos;
                //UnityEngine.Debug.Log(endPos);
            }
        }
    }
}