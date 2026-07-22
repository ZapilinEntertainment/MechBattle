using VContainer;
using Unity.Mathematics;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class VfxRequestsFactory : RequestFactoryBase<VfxRequestComponent>
    {
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<RotationComponent> _rotations;

        [Inject]
        public VfxRequestsFactory(World world) : base(world)
        {
            _positions = World.GetStash<PositionComponent>();
            _rotations = World.GetStash<RotationComponent>();
        }

        public void Build(VfxKey key, in RigidTransform transform) => Build(key, transform.pos, transform.rot);

        public void Build(VfxKey key, float3 pos, quaternion rot)
        {
            var requestEntity = Build(key,pos);
            _rotations.Set(requestEntity, new() { Value= rot });            
        }

        public Entity Build(VfxKey key, float3 pos)
        {
            var requestEntity = CreateRequest(new() { Value = key });
            _positions.Set(requestEntity, new() { Value= pos });
            return requestEntity;
        }
    
    }
}
