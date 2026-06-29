using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class WaypointsCheckSystem : PausableSystem
    {
        private Filter _filter;
        private Stash<PositionComponent> _positions;
        private Stash<WaypointMoveTarget> _waypoints;

        public WaypointsCheckSystem(SceneFlagsManager flags) : base(flags)
        {
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<WaypointMoveTarget>().Build();

            _positions = World.GetStash<PositionComponent>();
            _waypoints = World.GetStash<WaypointMoveTarget>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            foreach (var entity in _filter)
            {
                var waypointPosition = _waypoints.Get(entity).WorldPos;
                var position = _positions.Get(entity).Value;

                if (math.distancesq(position, waypointPosition) < math.EPSILON)
                {
                    _waypoints.Remove(entity);
                }
            }
        }
    }
}