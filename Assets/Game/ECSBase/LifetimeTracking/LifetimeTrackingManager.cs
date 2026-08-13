using R3;
using System.Collections.Generic;
using Scellecs.Morpeh;
using System;

namespace ZE.MechBattle
{
    public class LifetimeTrackingManager : IDisposable
    {
        private readonly Dictionary<Entity, DisposableBag> _lifetimeObjects = new();

        public void Dispose()
        {
            foreach (var kvp in _lifetimeObjects)
            {
                kvp.Value.Dispose();
            }
            _lifetimeObjects.Clear();
        }

        public DisposableBag GetEntityLifetimeObject(Entity entity)
        {
            if (!_lifetimeObjects.TryGetValue(entity, out var lifetimeObject))
            {
                lifetimeObject = new DisposableBag();
                _lifetimeObjects.Add(entity, lifetimeObject);
            }           
            return lifetimeObject;
        }

        public void OnEntityDisposed(Entity entity)
        {
            if (_lifetimeObjects.TryGetValue(entity, out var lifetimeObject))
                lifetimeObject.Dispose();
            _lifetimeObjects.Remove(entity);
        }
    
    }
}
