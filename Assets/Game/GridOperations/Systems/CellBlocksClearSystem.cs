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
        private Stash<CellsBlockerTag> _blockerTags;
        private Stash<CellPassabilityBlockComponent> _passabilityBlocks;
        private readonly HashSet<Entity> _clearList = new(8);

        public void OnAwake() 
        {
            _clearFilter = World.Filter
                .With<CellsBlockerTag>()
                .With<EntityDisposeTag>()
                .Build();

            _blockerTags = World.GetStash<CellsBlockerTag>();

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
                _clearList.Add(blockingEntity);
            }


            foreach (var cellEntity in _blockedCellsFilter)
            {
                var blockData = _passabilityBlocks.Get(cellEntity);

                CellOccupationLevelFlags primaryTokenOccupationFlags = CellOccupationLevelFlags.None;
                CellOccupationLevelFlags secondaryTokenOccupationFlags = CellOccupationLevelFlags.None;

                var isPrimaryTokenOutdated =
                    World.IsDisposed(blockData.SecondaryOccupationEntity)
                    || _clearList.Contains(blockData.PrimaryOccupationEntity);

                var isSecondaryTokenOutdated = 
                    World.IsDisposed(blockData.SecondaryOccupationEntity)
                    || _clearList.Contains(blockData.SecondaryOccupationEntity);

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
                        _passabilityBlocks.Set(cellEntity, new(blockData.PrimaryOccupationEntity, primaryTokenOccupationFlags));
                    }
                    else
                    {
                        // secondary token becomes primary
                        _passabilityBlocks.Set(cellEntity, new(blockData.SecondaryOccupationEntity, secondaryTokenOccupationFlags));
                    }
                }
            }

            foreach (var clearEntity in _clearList)
            {
                _blockerTags.Remove(clearEntity);
            }
            _clearList.Clear();
        }

        public void Dispose() { }
    }
}