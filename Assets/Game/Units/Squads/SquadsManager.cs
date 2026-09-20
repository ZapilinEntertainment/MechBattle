using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle
{
    public class SquadsManager
    {
        private int _nextSquadId = 1;
        private readonly Dictionary<int, Entity> _squads = new();

        public int RegisterNewSquad(Entity entity)
        {
            var id = _nextSquadId++;
            _squads.Add(id, entity);
            return id;
        }

        public bool TryGetSquad(int id, out Entity entity) => _squads.TryGetValue(id, out entity);

        public void RemoveSquad(int id) => _squads.Remove(id);
    }
}
