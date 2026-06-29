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

        private readonly MovementCellsList _vectorsList;
        private readonly float _invertedTriangleHeight;

        [Inject]
        public MovementCollisionAvoidanceSystem(SceneFlagsManager flags, MovementCellsList vectorsList, INavigationMap map) : base(flags)
        {
            _vectorsList = vectorsList;
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
                if (!_vectorsList.TryGetValue(nextTripos, out var moveCell))
                {
                    _vectorsList.Add(nextTripos, default);
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

                var currentPos = _positionComponents.Get(entity).Value;
                var moveDir = nextPosComponent.WorldPos - currentPos;
                var dot = math.dot(moveCell.MoveVector, moveDir);

                if (dot < 1f)
                {
                    // counter-direction
                    // need some rvo
                    SolveYieldingCase(entity, moveCell);
                }
                else
                {
                    var moveCf = dot * math.lengthsq(moveDir);
                    var nextPos = currentPos + moveCf * moveDir;
                    var resultingTripos = TriangularMath.WorldToTrianglePosInvertedHeight(nextPos, _invertedTriangleHeight);                                      
                    if (_vectorsList.TryGetValue(resultingTripos, out var alreadyOccupiedCell))
                    {
                        var currentTripos = _triangularPosComponents.Get(entity).Value;
                        _nextPositionComponents.Set(entity, new(currentPos, currentTripos));
                    }                        
                    else
                    {
                        _nextPositionComponents.Set(entity, new(nextPos, nextPosComponent.Tripos));
                        _vectorsList.Add(resultingTripos, default);
                    }                    
                }
            }
        }

        private void SearchForDetour(Entity entity)
        {
            //UnityEngine.Debug.Log($"need detour for {entity}");
            var currentPos = _positionComponents.Get(entity).Value;
            var nextPosComponent = new NextPositionComponent(currentPos, _triangularPosComponents.Get(entity).Value);
            _nextPositionComponents.Set(entity, nextPosComponent);
            //UnityEngine.Debug.Log($"entity {entity.Id} CAS SET: {currentPos} / {nextPosComponent.WorldPos}");
        }

        private void SolveYieldingCase(Entity entity, CellMovementData nextCellData)
        {
            //UnityEngine.Debug.Log($"solve yielding case for {entity}");
            _nextPositionComponents.Set(entity, new(_positionComponents.Get(entity).Value, _triangularPosComponents.Get(entity).Value));
        }
    }
}
