using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public struct EnergyCellsEnumerator
    {
        private bool _firstOneUsed;
        private readonly Entity _startEntity;
        private readonly Stash<NextEnergyCellComponent> _nextCellStash;

        public EnergyCellsEnumerator(Stash<NextEnergyCellComponent> stash, Entity startEntity)
        {
            _startEntity = startEntity;
            _nextCellStash = stash;
            Current = _startEntity;
            _firstOneUsed = false;
        }

        public Entity Current { get; private set; }

        public bool MoveNext()
        {
            if (!_firstOneUsed)
            {
                _firstOneUsed = true;
                return true;
            }                
            var nextCellComponent = _nextCellStash.Get(Current, out var nextCellExists);
            Current = nextCellComponent.CellEntity;

            return nextCellExists;
        }

        public EnergyCellsEnumerator GetEnumerator() => this;
    }
}
