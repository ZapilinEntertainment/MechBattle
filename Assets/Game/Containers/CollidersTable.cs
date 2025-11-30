using System;
using System.Collections.Generic;
using UnityEngine;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{

    public class CollidersTable : IDisposable
    {
        private readonly Dictionary<int, Entity> _table = new();

        public void RegisterCollider(Entity colliderOwner, int key) => _table[key] = colliderOwner;
        public void UnregisterCollider(int key) => _table.Remove(key);

        public bool TryGetColliderOwner(int key, out Entity colliderOwner) => _table.TryGetValue(key, out colliderOwner);

        public void Dispose()
        {
            _table.Clear();
        }    
    }
}
