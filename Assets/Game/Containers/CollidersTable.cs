using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Scellecs.Morpeh;

namespace ZE.MechBattle
{

    public class CollidersTable : IDisposable
    {
        // TODO: add session end clearing
        private readonly Dictionary<int, Entity> _table = new();
        private const int EXPECTED_MAX_COLLIDERS_COUNT = 8;

        public void RegisterCollider(Entity colliderOwner, int key) 
        {
            _table[key] = colliderOwner;
            //Debug.Log("registered: " + key.ToString());
        }
        public void UnregisterCollider(int key) => _table.Remove(key);
        public void UnregisterAllColliders(Entity owner)
        {
            Span<int> removeKeys = stackalloc int[EXPECTED_MAX_COLLIDERS_COUNT];
            var index = 0;
            foreach (var kvp in _table)
            {
                if (kvp.Value == owner)
                {
                    removeKeys[index++] = kvp.Key;
                    if (index >= EXPECTED_MAX_COLLIDERS_COUNT)
                    {
                        Debug.LogError($"Some collider have more than {EXPECTED_MAX_COLLIDERS_COUNT} colliders! Some of them might not be cleared!" );
                        break;
                    }                        
                }
            }

            if (index != 0)
            {
                for (var i = 0; i < index; i++)
                {
                    _table.Remove(i);
                }
            }
        }

        public bool TryGetColliderOwner(int key, out Entity colliderOwner) => _table.TryGetValue(key, out colliderOwner);

        public void Dispose()
        {
            _table.Clear();
        }    
    }
}
