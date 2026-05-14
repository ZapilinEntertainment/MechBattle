using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Jobs;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs 
{
    // selects the best of available hex paths (and request to calculate missing ones)
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class RegularHexPathSelectionSystem : ISystem 
    {
        public World World { get; set;}
        private readonly INavigationMap _map;          

        private readonly HexPathSearcher _hexPathSearcher;
        private int _lastAppliedMapVersion;

        private Filter _requestsFilter;        
        private Stash<HexPathSelectRequestComponent> _selectComponentsStash;
        private Stash<RegularHexPathComponent> _regularPathStash;        
        private const int CACHE_LIMIT = 32;

        [Inject]
        public RegularHexPathSelectionSystem(INavigationMap map, RequestedHexPathsList requestedHexPathsList, HexPathsLRUBuffer hexPaths)
        {
            _map = map;
           
            _lastAppliedMapVersion = map.Version;
            _hexPathSearcher = new(map, requestedHexPathsList, hexPaths, CACHE_LIMIT);;
        }

        public void OnAwake() 
        { 
            _requestsFilter = World.Filter
                .With<HexPathSelectRequestComponent>()
                .Without<ClearHexPathTag>()
                .Build();

            _selectComponentsStash = World.GetStash<HexPathSelectRequestComponent>();
            _regularPathStash = World.GetStash<RegularHexPathComponent>();

            _hexPathSearcher.OnMapVersionChanged();
            _lastAppliedMapVersion = _map.Version;
        }

        public void Dispose() { }

        public void OnUpdate(float deltaTime) 
        {            
            if (!_map.IsInitialized)
                return;            
                     
            HandleRequestingEntities();
        }

        private void HandleRequestingEntities()
        {
            if (!UpdateMapVersion())
                _hexPathSearcher.LeaveOnlyCalculatedPathsInCache();

            foreach (var entity in _requestsFilter)
            {
                var requestComponent = _selectComponentsStash.Get(entity);
                var startEdgesMask = requestComponent.StartEdgesMask;
                var endEdgesMask = requestComponent.EndEdgesMask;
                var startHexCoord = requestComponent.StartHex;
                var endHexCoord = requestComponent.EndHex;

               var resultData = _hexPathSearcher.GetHexPathData(startHexCoord, startEdgesMask, endHexCoord, endEdgesMask, requestMissedPathsCalculation: true);
                if (resultData.Result == HexPathSearcher.HexPathSearchResult.PathFound)
                {
                    UnityEngine.Debug.Log($"hex path set, target: {resultData.EndNode}, nodes count: {resultData.NodesCount}");
                    SetEntityHexPath(entity, resultData.PathId, resultData.NodesCount);
                }
                    
            }
        }

        private void SetEntityHexPath(Entity entity, int pathId, int nodesCount)
        {
            _regularPathStash.Set(entity, new RegularHexPathComponent(pathId, nodesCount));
            _selectComponentsStash.Remove(entity);
        }

        private bool UpdateMapVersion()
        {
            var currentMapVersion = _map.Version;
            if (currentMapVersion != _lastAppliedMapVersion)
            {
                _lastAppliedMapVersion = currentMapVersion;
                _hexPathSearcher.OnMapVersionChanged();
                return true;
            }
            return false;
        }
    }
}