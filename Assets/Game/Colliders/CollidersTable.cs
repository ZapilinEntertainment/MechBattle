using System;
using System.Collections.Generic;
using UnityEngine;
using Scellecs.Morpeh;
using VContainer;

namespace ZE.MechBattle
{

    public class CollidersTable : IDisposable
    {
        private readonly Dictionary<int, Entity> _table = new();
        private readonly World _world;
        private const int EXPECTED_MAX_COLLIDERS_COUNT = 8;

        [Inject]
        public CollidersTable(World world)
        {
            _world = world;
        }

        public void RegisterCollider(Entity colliderOwner, int key) 
        {
            _table[key] = colliderOwner;
            //Debug.Log($"bind collider key {key} to entity {colliderOwner.Id}");
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

        public bool TryGetColliderOwner(int key, out Entity colliderOwner)
        {
            if (_table.TryGetValue(key, out colliderOwner))
            {
                if (_world.IsDisposed(colliderOwner))
                {
                    _table.Remove(key);
                    colliderOwner = default;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            colliderOwner = default;
            return false;
        }

        public void Dispose()
        {
            _table.Clear();
        }    
    }
}
