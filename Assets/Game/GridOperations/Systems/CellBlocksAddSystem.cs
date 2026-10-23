using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CellBlocksAddSystem : ISystem 
    {
        public World World { get; set;}

        private bool _mapCalculated = false;
        private Filter _addFilter;
        private Stash<BlockZoneConfigComponent> _blockConfigs;
        private Stash<CellsBlockerTag> _blockTokens;

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly NavigationGridHandler _gridHandler;
        private readonly INavigationMap _navigationMap;

        [Inject]
        public CellBlocksAddSystem(TransformAspectHandler transformAspectHandler, NavigationGridHandler gridHandler, INavigationMap navigationMap)
        {
            _transformAspectHandler = transformAspectHandler;
            _gridHandler = gridHandler;
            _navigationMap = navigationMap;
        }

        public void OnAwake() 
        {
            _addFilter = World.Filter
                .With<BlockZoneConfigComponent>()
                .Without<CellsBlockerTag>()
                .Without<EntityDisposeTag>()
                .Build();

            _blockConfigs = World.GetStash<BlockZoneConfigComponent>();
            _blockTokens = World.GetStash<CellsBlockerTag>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (!_navigationMap.IsInitialized)
                return;

            foreach (var buildingEntity in _addFilter)
            {
                var blockData = _blockConfigs.Get(buildingEntity);
                var point = _transformAspectHandler.GetPoint(buildingEntity);

                _gridHandler.BlockCellsPassability(buildingEntity, blockData, point);
                _blockTokens.Add(buildingEntity);
            }
        }

        public void Dispose() { }
    }
}