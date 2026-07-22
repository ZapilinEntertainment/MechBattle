using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class NextPositionApplySystem : PausableSystem
    {
        private Filter _filter;
        private Stash<NextPositionComponent> _nextPositions;
        private Stash<PositionComponent> _positions;
        private readonly TransformAspectHandler _handler;
        private readonly INavigationMap _map;


        [Inject]
        public NextPositionApplySystem(SceneFlagsManager flags, TransformAspectHandler handler, INavigationMap map) : base(flags)
        {
            _handler = handler;
            _map = map;
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<NextPositionComponent>().Build();
            _nextPositions = World.GetStash<NextPositionComponent>();
            _positions = World.GetStash<PositionComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused) 
                return;

            foreach (var entity in _filter)
            {
                var nextPosComponent = _nextPositions.Get(entity);
                var currentPos = _positions.Get(entity).Value;
                if (math.all(nextPosComponent.WorldPosXZ == currentPos.xz))
                    continue;

                var nextPosXZ = nextPosComponent.WorldPosXZ;
                var triangleHeightData = _map.GetHeightData(nextPosComponent.Tripos);
                var localTripos = TriangularMath.WorldToTriangular(new float3(nextPosXZ.x, 0f, nextPosXZ.y), _map.TriangleHeight);
                var targetPos = new float3(
                    nextPosXZ.x, 
                    triangleHeightData.GetHeightAtPoint(nextPosComponent.Tripos, localTripos),
                    nextPosXZ.y);

               
                var fwd = math.normalize(targetPos - currentPos);
                var rotation = quaternion.LookRotationSafe(fwd, math.up());

                _handler.MoveToPoint(entity, targetPos, rotation);
            }
            _nextPositions.RemoveAll();
        }
    }
}