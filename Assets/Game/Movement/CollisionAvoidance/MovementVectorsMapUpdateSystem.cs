using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Navigation;
using Unity.Collections;
using Unity.Jobs;

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

        private readonly MovementCellsMap _vectorsList;
        private readonly INavigationMap _map;
        private readonly NativeList<IntTriangularPos> _resultsList;
      

        [Inject]
        public MovementVectorsMapUpdateSystem(
            SceneFlagsManager flags, 
            MovementCellsMap vectorsList,
            INavigationMap map) : base(flags)
        {
            _vectorsList = vectorsList;
            _map = map;

            _resultsList = new NativeList<IntTriangularPos>(Allocator.Persistent);
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
                var worldPos = _positionComponents.Get(entity).Value;                             

                var nextPosComponent = _nextPositionComponents.Get(entity, out var haveNextPos);
                float2 moveDir;
                if (haveNextPos)
                {
                    var nextPos = nextPosComponent.WorldPosXZ;
                    moveDir = nextPos - worldPos.xz;
                }
                else
                {
                    moveDir = float2.zero;
                }
               
                if (avoidanceComponent.RadiusInUnits == 0f)
                {
                    var currentCellData = new CellMovementData(
                   entity,
                   avoidanceComponent.Priority,
                   moveDir,
                   projectionIndex: 0);

                   _vectorsList.TryWriteCell(currentTripos, currentCellData);
                }
                else
                {
                   
                    var getListJob = new GetTrianglesInRadiusJob()
                    {
                        RadiusInUnits = avoidanceComponent.RadiusInUnits,
                        ResultList = _resultsList,
                        TriangleHeight = _map.TriangleHeight,
                        WorldPos = worldPos
                    };
                    getListJob.RunByRef();
                    foreach (var tripos in _resultsList)
                    {
                        var cellData = new CellMovementData(
                           entity,
                           avoidanceComponent.Priority,
                           moveDir,
                           projectionIndex: TriangularMath.CalculateDistance(tripos, currentTripos));
                        _vectorsList.TryWriteCell(tripos, cellData);
                    }
                    _resultsList.Clear();
                }
               
            }

            // move cells
            foreach (var entity in _movementProjectionsFilter)
            {
                var avoidanceComponent = _avoidanceComponents.Get(entity);
                var currentTripos = _triangularPosComponents.Get(entity).Value;
                var pos = _positionComponents.Get(entity).Value.xz;

                var nextPosComponent = _nextPositionComponents.Get(entity);
                var nextPos = nextPosComponent.WorldPosXZ;
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

        protected override void InternalDispose()
        {
            base.InternalDispose();
#if UNITY_EDITOR
            try
            {
#endif
                _resultsList.Dispose();
#if UNITY_EDITOR
            }
            catch
            {
                // editor dispose problems
            }
#endif
            }
    }
}