using Scellecs.Morpeh;
using Scellecs.Morpeh.Native;
using Unity.Collections;
using Unity.IL2CPP.CompilerServices;
using Unity.Jobs;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class ViewVisibilityHexSectorCheckSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _calculatedEntitiesFilter;
        private Filter _uncalculatedEntitiesFilter;
        private Stash<HexCoordComponent> _hexCoords;
        private Stash<TriangularPosComponent> _tripos;
        private Stash<ViewVisibilityStateComponent> _visibilityStates;
        private Stash<PositionComponent> _positions;

        private NativeHashMap<int2, int> _sectorVisibilityMasks;
        private readonly IVisibleHexList _visibleHexList;
        private readonly INavigationMap _map;
        private const int REMOVE_SECTOR_VISIBILITY_MASK = ~((int)VisibilityStateFlag.HexSectorVisible);

        [Inject]
        public ViewVisibilityHexSectorCheckSystem(INavigationMap navigationMap, IVisibleHexList visibleHexList)
        {
            _map = navigationMap;
            _visibleHexList = visibleHexList;

            _sectorVisibilityMasks = new NativeHashMap<int2, int>(64, Allocator.Persistent);
        }

        public void OnAwake() 
        {
            _calculatedEntitiesFilter = World.Filter
                .With<HexCoordComponent>()
                .With<TriangularPosComponent>()
                .With<ViewVisibilityStateComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _uncalculatedEntitiesFilter = World.Filter
                .With<ViewVisibilityStateComponent>()
                .Without<TriangularPosComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _hexCoords = World.GetStash<HexCoordComponent>();
            _tripos = World.GetStash<TriangularPosComponent>();
            _visibilityStates = World.GetStash<ViewVisibilityStateComponent>();
            _positions = World.GetStash<PositionComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            var hexEdgeLength = _map.HexEdgeLength;
            var hexRadius = _map.TrianglesPerHexEdge;
            var triangleHeight = _map.TriangleHeight;

            foreach (var entity in _calculatedEntitiesFilter)
            {
                var hexCoord = _hexCoords.Get(entity).Value;
                ref var visibilityState = ref _visibilityStates.Get(entity);
                if (!_visibleHexList.IsHexVisible(hexCoord, out var sectorsVisibilityMask))
                {
                    visibilityState.Value = RemoveSectorVisibilityMask(visibilityState.Value);
                    continue;
                }

                var tripos = _tripos.Get(entity).Value;
                var sector = (int)HexMath.DefineSectorByTripos(tripos, hexEdgeLength, hexRadius, triangleHeight);
                if ((sectorsVisibilityMask & (1 << sector)) != 0)
                {
                    visibilityState.Value |= VisibilityStateFlag.HexSectorVisible;
                }                    
                else
                    visibilityState.Value = RemoveSectorVisibilityMask(visibilityState.Value);

            }

            if (_uncalculatedEntitiesFilter.IsNotEmpty())
            {
                _sectorVisibilityMasks.Clear();
                foreach (var visibleSectorKvp in _visibleHexList.VisibleSectors)
                {
                    _sectorVisibilityMasks.Add(visibleSectorKvp.Key, visibleSectorKvp.Value);
                }

                var job = new CheckViewVisibilityByHexSectorJob()
                {
                    Filter = _uncalculatedEntitiesFilter.AsNative(),
                    HexEdgeLength = hexEdgeLength,
                    SectorVisibilityMasks = _sectorVisibilityMasks,
                    HexRadius = hexRadius,
                    Positions = _positions.AsNative(),
                    TriangleHeight = triangleHeight,
                    VisibilityStates = _visibilityStates.AsNative()
                };
                World.JobHandle = job.Schedule();
            }
        }

        public void Dispose() 
        {
            _sectorVisibilityMasks.Dispose();
        }

        private VisibilityStateFlag RemoveSectorVisibilityMask(VisibilityStateFlag originalMask) =>
            (VisibilityStateFlag)((int)originalMask & REMOVE_SECTOR_VISIBILITY_MASK);
    }
}