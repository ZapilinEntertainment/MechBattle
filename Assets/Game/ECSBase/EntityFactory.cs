using UnityEngine;
using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    public class EntityFactory
    {
        private readonly World _world;
        private readonly TransformAccessManager _transformAccessManager;
        private readonly Stash<TransformComponent> _transforms;
        private readonly Stash<PositionComponent> _positions;
        private readonly Stash<RotationComponent> _rotations;
        private readonly Stash<ViewComponent> _views;

        [Inject]
        public EntityFactory(TransformAccessManager accessManager, World world)
        {
            _world = world;
            _transformAccessManager = accessManager;

            _transforms = _world.GetStash<TransformComponent>();
            _positions = _world.GetStash<PositionComponent>();
            _rotations = _world.GetStash<RotationComponent>();
            _views = _world.GetStash<ViewComponent>();
        }

        public Entity Build(IMonoView view)
        {
            var entity = _world.CreateEntity();

            var transform = view.Transform;
            var key = _transformAccessManager.RegisterTransform(transform);
            _transforms.Set(entity, new() { Key = key });

            _positions.Set(entity, new() { Value = transform.position});
            _rotations.Set(entity, new() { Value = transform.rotation});

            _views.Set(entity, new() { Value = view});

            return entity;
        }
    
    }
}
