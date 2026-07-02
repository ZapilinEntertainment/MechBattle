using Scellecs.Morpeh;
using VContainer;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle.Ecs
{
    public class UnitSpawnRequestsFactory
    {
        private readonly World _world;
        private readonly Stash<SpawnRequestComponent> _spawnRequests;

        [Inject]
        public UnitSpawnRequestsFactory(World world)
        {
            _world = world;
        }

        public void CreateSpawnRequest(UnitKey unitKey, IntTriangularPos tripos, PlayerKey playerKey)
        {
            var entity = _world.CreateEntity();
            _spawnRequests.Add(entity, new(unitKey, tripos, playerKey));
        }
    
    }
}
