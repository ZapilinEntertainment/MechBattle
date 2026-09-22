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

        private readonly IMovementCellsMap _movementCells;
        private readonly float _invertedTriangleHeight;

        [Inject]
        public MovementCollisionAvoidanceSystem(SceneFlagsManager flags, IMovementCellsMap movementCells, INavigationMap map) : base(flags)
        {
            _movementCells = movementCells;
            _invertedTriangleHeight = map.InvertedTriangleHeight;
        }

        public override void OnAwake()
        {
            _filter = World.Filter.With<NextPositionComponent>().With<MovementCollisionAvoidanceComponent>().Build();

            _nextPositionComponents = World.GetStash<NextPositionComponent>();
            _positionComponents = World.GetStash<PositionComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;            

            foreach (var entity in _filter)
            {
                var nextPosComponent = _nextPositionComponents.Get(entity);
                var nextTripos = nextPosComponent.Tripos;
                var currentPos = _positionComponents.Get(entity).Value.xz;
                var moveDir = nextPosComponent.WorldPosXZ - currentPos;

                if (!_movementCells.TryGetValue(nextTripos, out var moveCell))
                {
                    _movementCells.TryWriteCell(nextTripos, entity, moveDir, 0);
                    continue;
                }
                else
                {
                    if (moveCell.Entity == entity)
                    {
                        continue;
                    }
                        
                }

                if (math.lengthsq( moveCell.MoveVector) == 0f)
                {
                    SearchForDetour(entity);
                    continue;
                }

                
                var dot = math.dot(moveCell.MoveVector, moveDir);

                if (dot < 1f)
                {
                    // counter-direction
                    // possible solution: half-speed, but co-existance in same cell
                    SolveYieldingCase(entity, moveCell);
                }
                else
                {
                    var moveCf = dot * math.lengthsq(moveDir);
                    var nextPos = currentPos + moveCf * moveDir;
                    var resultingTripos = TriangularMath.WorldToTrianglePosInvertedHeight(new float3(nextPos.x, 0f, nextPos.y), _invertedTriangleHeight);                                      
                    if (_movementCells.TryGetValue(resultingTripos, out var alreadyOccupiedCell))
                    {
                        var currentTripos = _triangularPosComponents.Get(entity).Value;
                        _nextPositionComponents.Set(entity, new(currentPos, currentTripos));
                    }                        
                    else
                    {
                        _nextPositionComponents.Set(entity, new(nextPos, nextPosComponent.Tripos));
                        _movementCells.TryWriteCell(resultingTripos, entity, moveDir, 1);
                    }                    
                }
            }
        }

        private void SearchForDetour(Entity entity)
        {
            //UnityEngine.Debug.Log($"need detour for {entity}");
            var currentPos = _positionComponents.Get(entity).Value.xz;
            var nextPosComponent = new NextPositionComponent(currentPos, _triangularPosComponents.Get(entity).Value);
            _nextPositionComponents.Set(entity, nextPosComponent);
            //UnityEngine.Debug.Log($"entity {entity.Id} CAS SET: {currentPos} / {nextPosComponent.WorldPos}");
        }

        private void SolveYieldingCase(Entity entity, CellMovementData nextCellData)
        {
            //UnityEngine.Debug.Log($"solve yielding case for {entity}");
            _nextPositionComponents.Set(entity, new(_positionComponents.Get(entity).Value.xz, _triangularPosComponents.Get(entity).Value));
        }
    }
}
