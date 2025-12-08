using VContainer;
using UnityEngine;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs.Pathfinding
{
    public class PathfinderFactory
    {
        private readonly World _world;

        [Inject]
        public PathfinderFactory(World world)
        {
            _world = world;
        }

        public void AddPathfinderToGroup(Entity groupEntity, int groupId)
        {
        }
    
    }
}
