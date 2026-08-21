using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle.GridOperations
{
    public class UnitsGrid : IUnitsGrid
    {
        private class HexUnitsList
        {
            public IReadOnlyList<Entity> UnitsList => _entities;
            private readonly List<Entity> _entities = new();

            public void Add(Entity entity) => _entities.Add(entity);
            public void Clear() => _entities.Clear();
        }

        public bool IsEmpty { get; private set; } = true;
        private readonly Dictionary<int2, HexUnitsList> _hexUnitLists = new();

        public void Clear()
        {
            foreach (var list in _hexUnitLists.Values)
            {
                list.Clear();
            }
            IsEmpty = true;
        }

        public void AddUnit(Entity entity, int2 hexCoord)
        {
            if (!_hexUnitLists.TryGetValue(hexCoord, out var list))
            {
                list = new HexUnitsList();
                _hexUnitLists.Add(hexCoord, list);
            }
            list.Add(entity);
            IsEmpty = false;
        }

        public bool TryGetUnitsInHex(int2 hexCoord, out IReadOnlyList<Entity> entitiesList)
        {
            if (_hexUnitLists.TryGetValue(hexCoord, out var hexUnitsList))
            {
                entitiesList = hexUnitsList.UnitsList;
                return true;
            }
            else
            {
                entitiesList = null;
                return false;
            }
        }
            
    
    }
}
