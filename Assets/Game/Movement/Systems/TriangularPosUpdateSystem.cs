using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class TriangularPosUpdateSystem : ISystem 
    {
        public World World { get; set;}
        public const SystemGroupOrder GroupOrder = SystemGroupOrder.RegularUpdate;

        private Filter _filter;
        private Stash<TriangularPosComponent> _tripos;
        private Stash<PositionComponent> _positions;
        private Stash<HexCoordComponent> _hexCoords;

        private readonly float _triangleHeightInverted;
        private readonly float _hexEdge;

        [Inject]
        public TriangularPosUpdateSystem(INavigationMap map)
        {
            _triangleHeightInverted = 1f / map.TriangleHeight;
            _hexEdge = map.HexEdgeLength;
        }

        public void OnAwake() 
        {
            _filter = World.Filter.With<NavigationAgentComponent>().Build();

            _tripos = World.GetStash<TriangularPosComponent>();
            _positions = World.GetStash<PositionComponent>();
            _hexCoords = World.GetStash<HexCoordComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var entity in _filter)
            {
                var worldPos = _positions.Get(entity).Value;

                var tripos = TriangularMath.WorldToTrianglePosInvertedHeight(worldPos, _triangleHeightInverted);
                _tripos.Set(entity, new() { Value = tripos});

                _hexCoords.Set(entity, new() {Value = HexMath.DefineHex(worldPos.xz, _hexEdge) });
            }
        }

        public void Dispose() { }
    }
}