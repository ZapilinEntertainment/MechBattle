using UnityEngine;
using VContainer;
using Scellecs.Morpeh;

namespace ZE.MechBattle.Ecs
{
    // builds Morpeh entities from existing on-scene views
    public class ViewSynchronizationApplier
    {
        private readonly World _world;
        private readonly TransformAccessManager _transformAccessManager;
        private readonly Stash<TransformComponent> _transforms;
        private readonly TransformAspectHandler _transformAspectHandler;

        [Inject]
        public ViewSynchronizationApplier(TransformAccessManager accessManager, World world, TransformAspectHandler transformAspectHandler)
        {
            _world = world;
            _transformAccessManager = accessManager;
            _transformAspectHandler = transformAspectHandler;

            _transforms = _world.GetStash<TransformComponent>();
        }

        public void Apply(Entity entity, IMonoView view)
        {
            var transform = view.Transform;
            transform.SetParent(null);
            var key = _transformAccessManager.RegisterTransform(transform);
            _transforms.Set(entity, new() { Key = key });
            _transformAspectHandler.UpdatePoint(entity, transform);

#if UNITY_EDITOR
            view.name = $"entity {entity.Id}";
#endif
        }

    }
}

