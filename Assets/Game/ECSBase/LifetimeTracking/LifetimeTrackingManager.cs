using R3;
using System.Collections.Generic;
using Scellecs.Morpeh;
using System;
using ZE.MechBattle.Ecs;
using VContainer;

namespace ZE.MechBattle
{
    public class LifetimeTrackingManager : IDisposable
    {
        private readonly Dictionary<Entity, DisposableBag> _lifetimeObjects = new();
        private readonly Stash<LifetimeTrackingTag> _lifetimeTrackingTags;

        [Inject]
        public LifetimeTrackingManager(World world)
        {
            _lifetimeTrackingTags = world.GetStash<LifetimeTrackingTag>();
        }

        public void Dispose()
        {
            foreach (var kvp in _lifetimeObjects)
            {
                kvp.Value.Dispose();
            }
            _lifetimeObjects.Clear();
        }

        public void AddToEntityLifetime<T>(Entity entity, T obj) where T : IDisposable
        {
            if (!_lifetimeObjects.TryGetValue(entity, out var lifetimeObject))
                lifetimeObject = new DisposableBag();
            lifetimeObject.Add(obj);
            // NOTE: reassignment required - despite using list in structure, it also holds last element index, that will not be saved otherwise
            _lifetimeObjects[entity] = lifetimeObject;

            _lifetimeTrackingTags.Set(entity);
        }

        public void OnEntityDisposed(Entity entity)
        {
            if (_lifetimeObjects.TryGetValue(entity, out var lifetimeObject))
                lifetimeObject.Dispose();
            _lifetimeObjects.Remove(entity);
        }
    
    }
}
