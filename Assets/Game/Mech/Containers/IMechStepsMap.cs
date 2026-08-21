using Scellecs.Morpeh;
using System.Collections.Generic;
using ZE.MechBattle.Navigation;
using ZE.MechBattle.MechMovement;
using System.Collections;

namespace ZE.MechBattle
{
    public interface IMechStepsMap : IEnumerable<KeyValuePair<IntTriangularPos, Entity>>
    {
        bool IsEmpty { get; }

        void Update(IMechStepsAffectionMapSource source);
        bool TryGetAffectionData(Entity entity, out StepAffectionData affectionData);
    }

    public struct StepAffectionData
    {
        public int TotalCellsCount;
        public int SuitableCellsCount;
    }
}


namespace ZE.MechBattle.MechMovement
{
    public class MechStepsMap : IMechStepsMap
    {
        public bool IsEmpty => _alreadyOccupiedCells.Count == 0;

        private readonly Dictionary<Entity, StepAffectionData> _affectionData = new(INITIAL_CAPACITY);
        private readonly Dictionary<IntTriangularPos, Entity> _alreadyOccupiedCells = new(INITIAL_CAPACITY);
        private const int INITIAL_CAPACITY = 32;

        public void Update(IMechStepsAffectionMapSource source)
        {
            _affectionData.Clear();
            _alreadyOccupiedCells.Clear();
            source.GetStepAffectedCells(AddAffectionData);
        }

        public bool TryGetAffectionData(Entity entity, out StepAffectionData affectionData) => 
            _affectionData.TryGetValue(entity, out affectionData);

        private void AddAffectionData(IntTriangularPos tripos, Entity entity)
        {
            _affectionData.TryGetValue(entity, out var affectionData);
            affectionData.TotalCellsCount++;

            if (_alreadyOccupiedCells.TryAdd(tripos, entity))
                affectionData.SuitableCellsCount++;

            _affectionData[entity] = affectionData;
        }

        public IEnumerator<KeyValuePair<IntTriangularPos, Entity>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<IntTriangularPos, Entity>>)_alreadyOccupiedCells).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_alreadyOccupiedCells).GetEnumerator();
        }
    }
}
