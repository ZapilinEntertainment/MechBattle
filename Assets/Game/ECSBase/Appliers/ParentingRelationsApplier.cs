using Scellecs.Morpeh;
using VContainer;
using Unity.Mathematics;

namespace ZE.MechBattle.Ecs
{
    public class ParentingRelationsApplier
    {
        public struct ExecutionProtocol
        {
            public Entity ParentEntity;
            public Entity ChildEntity;
            public float3 LocalPos;
            public quaternion LocalRot;
        }

        private readonly Stash<ParentEntityComponent> _parents;
        private readonly Stash<LocalPositionComponent> _localPositions;
        private readonly Stash<LocalRotationComponent> _localRotation;

        [Inject]
        public ParentingRelationsApplier(World world)
        {
            _parents = world.GetStash<ParentEntityComponent>();
            _localPositions = world.GetStash<LocalPositionComponent>();
            _localRotation = world.GetStash<LocalRotationComponent>();
        }

        public void Apply(ExecutionProtocol protocol)
        {
            _parents.Set(protocol.ChildEntity, new(protocol.ParentEntity));
            _localPositions.Set(protocol.ChildEntity, new() { Value = protocol.LocalPos});
            _localRotation.Set(protocol.ChildEntity, new() { Value = protocol.LocalRot});
        }
    
    }
}
