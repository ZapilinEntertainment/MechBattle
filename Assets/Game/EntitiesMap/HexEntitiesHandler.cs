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
        private readonly NavigationHexEntitiesFactory _hexFactory;
        private readonly Stash<PassabilityVersionComponent> _passabilityVersion;

        [Inject]
        public HexEntitiesHandler(World world, NavigationHexEntitiesFactory hexEntitiesFactory)
        {
            _world = world;
            _hexFactory = hexEntitiesFactory;
            _passabilityVersion = _world.GetStash<PassabilityVersionComponent>();
        }

        public Entity CreateHexEntity(int2 hexCoord) => _hexFactory.Build(hexCoord);

        public int GetPassabilityVersion(Entity entity) => 
            GetComponentOrDefaultValueCommand
            .Execute(entity, _passabilityVersion, 0);

        public void IncreasePassabilityVersion(Entity entity) =>
            IncreaseComponentValueCommand
            .Execute(entity, _passabilityVersion);
    
    }
}
