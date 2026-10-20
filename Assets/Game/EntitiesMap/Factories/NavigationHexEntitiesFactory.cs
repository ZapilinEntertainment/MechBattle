using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class NavigationHexEntitiesFactory : NavigationDataEntitiesFactoryBase<INavigationHexCreationApplier>
    {
        private Stash<HexComponent> _hexes;

        public NavigationHexEntitiesFactory(World world, IEnumerable<INavigationHexCreationApplier> creationAppliers) : base(world, creationAppliers)
        {
        }

        public Entity Build(int2 hexCoord)
        {
            var entity = BuildWithAppliers();
            _hexes.Add(entity, new(hexCoord));
            return entity;
        }

        protected override void Init()
        {
            _hexes = _world.GetStash<HexComponent>();            
        }
    }
}
