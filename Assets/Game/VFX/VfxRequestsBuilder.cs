using VContainer;
using Unity.Mathematics;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class VfxRequestsBuilder
    {
        private readonly World _world;
        private readonly Stash<VirtualPositionComponent> _positions;
        private readonly Stash<VirtualRotationComponent> _rotations;
        private readonly Stash<VfxRequestComponent> _requests;

        [Inject]
        public VfxRequestsBuilder(World world)
        {
            _world = world;
            _positions = _world.GetStash<VirtualPositionComponent>();
            _rotations = _world.GetStash<VirtualRotationComponent>();
            _requests = _world.GetStash<VfxRequestComponent>();
        }


        public void Build(VfxKey key, float3 pos, quaternion rot)
        {
            var entity = _world.CreateEntity();
            _positions.Set(entity, new() { Value= pos });
            _rotations.Set(entity, new() { Value= rot });
            _requests.Set(entity, new() { Value = key});
        }

         public void Build(VfxKey key, float3 pos)
        {
            var entity = _world.CreateEntity();
            _positions.Set(entity, new() { Value= pos });
            _requests.Set(entity, new() { Value = key});
        }
    
    }
}
