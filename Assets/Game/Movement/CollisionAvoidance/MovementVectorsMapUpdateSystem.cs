using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class MovementVectorsMapUpdateSystem : PausableSystem 
    {
        private Filter _movementProjectionsFilter;
        private Filter _occupationCellsFilter;
        private Stash<MovementCollisionAvoidanceComponent> _avoidanceComponents;
        private Stash<PositionComponent> _positionComponents;
        private Stash<NextPositionComponent> _nextPositionComponents;
        private Stash<TriangularPosComponent> _triangularPosComponents;

        private readonly MovementCellsList _vectorsList;
      

        [Inject]
        public MovementVectorsMapUpdateSystem(
            SceneFlagsManager flags, 
            MovementCellsList vectorsList) : base(flags)
        {
            _vectorsList = vectorsList;
        }

        public override void OnAwake()
        {
            _movementProjectionsFilter = World.Filter.With<MovementCollisionAvoidanceComponent>().With<NextPositionComponent>().Build();
            _occupationCellsFilter = World.Filter.With<MovementCollisionAvoidanceComponent>().Build();

            _avoidanceComponents = World.GetStash<MovementCollisionAvoidanceComponent>();
            _triangularPosComponents = World.GetStash<TriangularPosComponent>();
            _nextPositionComponents = World.GetStash<NextPositionComponent>();
            _positionComponents = World.GetStash<PositionComponent>();
        }

        public override void OnUpdate(float deltaTime)
        {
            if (IsPaused)
                return;

            _vectorsList.Clear();

            // current occupation cells
            foreach (var entity in _occupationCellsFilter)
            {
                var avoidanceComponent = _avoidanceComponents.Get(entity);
                var currentTripos = _triangularPosComponents.Get(entity).Value;                
                var pos = _positionComponents.Get(entity).Value;

                var nextPosComponent = _nextPositionComponents.Get(entity, out var haveNextPos);
                float3 moveDir;
                if (haveNextPos)
                {
                    var nextPos = nextPosComponent.WorldPos;
                    moveDir = nextPos - pos;
                }
                else
                {
                    moveDir = float3.zero;
                }
               

                var currentCellData = new CellMovementData(
                    entity,
                    avoidanceComponent.Priority,
                    moveDir,
                    projectionIndex: 0 );

                if (!_vectorsList.TryWriteCell(currentTripos, currentCellData))
                {
                    #if UNITY_EDITOR
                    UnityEngine.Debug.LogError("entity move cell overlap");
                    #endif
                }
            }

            // move cells
            foreach (var entity in _movementProjectionsFilter)
            {
                var avoidanceComponent = _avoidanceComponents.Get(entity);
                var currentTripos = _triangularPosComponents.Get(entity).Value;
                var pos = _positionComponents.Get(entity).Value;

                var nextPosComponent = _nextPositionComponents.Get(entity);
                var nextPos = nextPosComponent.WorldPos;
                var moveDir = nextPos - pos;

                var nextTripos = nextPosComponent.Tripos;
                if (nextTripos != currentTripos)
                {
                    // NOTE: count only on 1 neighbour triangle cell, will not work for very fast objects
                    var nextCellData = new CellMovementData(
                        entity,
                        avoidanceComponent.Priority,
                        moveDir,
                        projectionIndex: 1);
                    _vectorsList.TryWriteCell(nextTripos, nextCellData);
                }
            }
        }
    }
}