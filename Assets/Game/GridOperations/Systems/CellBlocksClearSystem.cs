using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using VContainer;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class CellBlocksClearSystem : ISystem 
    {
        public World World { get; set;}
        private Filter _clearFilter;
        private Filter _blockedCellsFilter;
        private Stash<CellsBlockTokenComponent> _blockTokens;
        private Stash<CellPassabilityBlockComponent> _passabilityBlocks;
        private readonly HashSet<int> _clearList = new(8);
        private readonly CellBlockTokensList _cellBlockTokensList;

        [Inject]
        public CellBlocksClearSystem(CellBlockTokensList tokensList)
        {
            _cellBlockTokensList = tokensList;
        }

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<CellsBlockTokenComponent>()
                .With<EntityDisposeTag>()
                .Build();

            _blockTokens = World.GetStash<CellsBlockTokenComponent>();

            _blockedCellsFilter = World.Filter
                .With<CellEntityComponent>()
                .With<CellPassabilityBlockComponent>()
                .Build();

            _passabilityBlocks = World.GetStash<CellPassabilityBlockComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_clearFilter.IsEmpty() || _blockedCellsFilter.IsEmpty())
                return;

            foreach (var blockingEntity in _clearFilter)
            {
                _clearList.Add(_blockTokens.Get(blockingEntity).TokenId);
            }


            foreach (var cellEntity in _blockedCellsFilter)
            {
                var blockData = _passabilityBlocks.Get(cellEntity);

                CellOccupationLevelFlags primaryTokenOccupationFlags = CellOccupationLevelFlags.None;
                CellOccupationLevelFlags secondaryTokenOccupationFlags = CellOccupationLevelFlags.None;

                var isPrimaryTokenOutdated = 
                    _clearList.Contains(blockData.PrimaryOccupationTokenId)
                    || !_cellBlockTokensList.TryGetValue(blockData.PrimaryOccupationTokenId, out primaryTokenOccupationFlags);

                var isSecondaryTokenOutdated = blockData.SecondaryOccupationTokenId == -1
                        || _clearList.Contains(blockData.SecondaryOccupationTokenId)
                        || !_cellBlockTokensList.TryGetValue(blockData.SecondaryOccupationTokenId, out secondaryTokenOccupationFlags);

                if (isPrimaryTokenOutdated & isSecondaryTokenOutdated)
                {
                    // clear block completely
                    _passabilityBlocks.Remove(cellEntity);
                }
                else
                {
                    if (isSecondaryTokenOutdated)
                    {
                        // clear secondaryToken
                        _passabilityBlocks.Set(cellEntity, new(blockData.PrimaryOccupationTokenId, primaryTokenOccupationFlags));
                    }
                    else
                    {
                        // secondary token becomes primary
                        _passabilityBlocks.Set(cellEntity, new(blockData.SecondaryOccupationTokenId, secondaryTokenOccupationFlags));
                    }
                }
            }

            foreach (var clearTokenId in _clearList)
            {
                _cellBlockTokensList.Remove(clearTokenId);
            }
            _clearList.Clear();
        }

        public void Dispose()
        {

        }
    }
}