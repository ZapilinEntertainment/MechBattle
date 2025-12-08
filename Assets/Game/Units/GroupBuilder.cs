using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs.States;
using ZE.MechBattle.Ecs.Pathfinding;

namespace ZE.MechBattle.Ecs
{
    public class GroupBuilder
    {
        private readonly World _world;
        private readonly PathfinderFactory _pathfinderFactory;
        private readonly Stash<StateComponent> _states;
        private readonly Stash<BehaviourKeyComponent> _behaviour;
        private readonly Stash<UnitGroupComponent> _unitGroups;
        private readonly Stash<GroupEntityTag> _groupTags;

        [Inject]
        public GroupBuilder(World world, PathfinderFactory pathfinderFactory) 
        { 
            _world = world;
            _pathfinderFactory = pathfinderFactory;

            _states = _world.GetStash<StateComponent>();
            _behaviour = _world.GetStash<BehaviourKeyComponent>();
            _unitGroups = _world.GetStash<UnitGroupComponent>();
            _groupTags = _world.GetStash<GroupEntityTag>();
        }

        public Entity BuildGroup(int index)
        {
            var entity = _world.CreateEntity();
            _behaviour.Set(entity, new() { Value = BehaviourKey.TankGroup});
            _states.Add(entity);
            _unitGroups.Set(entity, new() { GroupId = index, UnitIndex = -1});
            _groupTags.Add(entity);

            _pathfinderFactory.AddPathfinderToGroup(entity, index);
            return entity;
        }
    
    }
}
