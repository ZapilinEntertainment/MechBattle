using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public struct EnergyCellsEnumerator
    {
        private readonly Entity _startEntity;
        private readonly Stash<NextEnergyCellComponent> _nextCellStash;

        public EnergyCellsEnumerator(Stash<NextEnergyCellComponent> stash, Entity startEntity)
        {
            _startEntity = startEntity;
            _nextCellStash = stash;
            Current = default;
        }

        public Entity Current { get; private set; }

        public bool MoveNext()
        {
            var nextCellComponent = _nextCellStash.Get(_startEntity, out var nextCellExists);
            Current = nextCellComponent.CellEntity;

            return nextCellExists;
        }

        public EnergyCellsEnumerator GetEnumerator() => this;
    }
}
