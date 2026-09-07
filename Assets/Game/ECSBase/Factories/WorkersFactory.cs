using Scellecs.Morpeh;
using UnityEngine;
using VContainer;
using ZE.Workers;

namespace ZE.MechBattle
{
    public class WorkersFactory
    {
        private readonly World _world;
        private readonly LifetimeTrackingManager _lifetimeTrackingManager;
        private readonly IObjectResolver _objectResolver;

        [Inject]
        public WorkersFactory(LifetimeTrackingManager lifetimeTrackingManager, IObjectResolver resolver, World world)
        {
            _world = world;
            _objectResolver = resolver;
            _lifetimeTrackingManager = lifetimeTrackingManager;
        }

        public T AddWorkerToEntity<T>(Entity entity) where T : Worker
        {
            if (_world.IsDisposed(entity))
                throw new EntityDisposedException();

            var worker = CreateWorker<T>();
            var lifetimeObject = _lifetimeTrackingManager.GetEntityLifetimeObject(entity);
            lifetimeObject.Add(worker);
            return worker;
        }

        public T CreateWorker<T>() where T : Worker => _objectResolver.Resolve<T>();

    }
}
