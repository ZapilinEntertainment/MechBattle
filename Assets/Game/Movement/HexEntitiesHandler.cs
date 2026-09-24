using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Navigation.Ecs
{
    public class HexEntitiesHandler
    {
        private readonly World _world;
        private readonly Stash<HexComponent> _hexes;
        private readonly Stash<PassabilityVersionComponent> _passabilityVersion;

        [Inject]
        public HexEntitiesHandler(World world)
        {
            _world = world;
            _hexes = _world.GetStash<HexComponent>();
            _passabilityVersion = _world.GetStash<PassabilityVersionComponent>();
        }

        public Entity CreateHexEntity(int2 hexCoord)
        {
            var entity = _world.CreateEntity();
            _hexes.Set(entity, new(hexCoord));
            return entity;
        }

        public int GetPassabilityVersion(Entity entity) => 
            GetComponentOrDefaultValueCommand
            .Execute(entity, _passabilityVersion, 0);

        public void IncreasePassabilityVersion(Entity entity) =>
            IncreaseComponentValueCommand
            .Execute(entity, _passabilityVersion);
    
    }
}
