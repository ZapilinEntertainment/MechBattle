using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CellBlocksAddSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _addFilter;
        private Stash<BlockZoneConfigComponent> _blockConfigs;
        private Stash<CellsBlockTokenComponent> _blockTokens;

        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly NavigationGridHandler _gridHandler;

        [Inject]
        public CellBlocksAddSystem(TransformAspectHandler transformAspectHandler, NavigationGridHandler gridHandler)
        {
            _transformAspectHandler = transformAspectHandler;
            _gridHandler = gridHandler;
        }

        public void OnAwake() 
        {
            _addFilter = World.Filter
                .With<BlockZoneConfigComponent>()
                .Without<CellsBlockTokenComponent>()
                .Without<EntityDisposeTag>()
                .Build();

            _blockConfigs = World.GetStash<BlockZoneConfigComponent>();
            _blockTokens = World.GetStash<CellsBlockTokenComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            foreach (var buildingEntity in _addFilter)
            {
                var blockData = _blockConfigs.Get(buildingEntity);
                var point = _transformAspectHandler.GetPoint(buildingEntity);

                var tokenId = _gridHandler.BlockCellsPassability(blockData, point);
                _blockTokens.Add(buildingEntity, new(tokenId));
            }
        }

        public void Dispose() { }
    }
}