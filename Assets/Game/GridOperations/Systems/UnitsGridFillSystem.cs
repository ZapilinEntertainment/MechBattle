using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.GridOperations;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class UnitsGridFillSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _filter;
        private Stash<HexCoordComponent> _hexCoords;
        private readonly UnitsGrid _unitsGrid;

        [Inject]
        public UnitsGridFillSystem(IUnitsGrid unitsGrid)
        {
            _unitsGrid = unitsGrid as UnitsGrid;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<HexCoordComponent>()
                .With<UnitTag>()
                .Without<EntityDisposeTag>()
                .Build();

            _hexCoords = World.GetStash<HexCoordComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (!_unitsGrid.IsEmpty)
                _unitsGrid.Clear();

            foreach (var entity in _filter)
            {
                var hexCoord = _hexCoords.Get(entity).Value;
                _unitsGrid.AddUnit(entity, hexCoord);
            }
        }

        public void Dispose() { }
    }
}