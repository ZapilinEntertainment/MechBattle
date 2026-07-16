using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    public class ColliderOwnityApplier
    {
        private readonly CollidersTable _collidersTable;
        private readonly Stash<RegisteredCollidersOwnerTag> _colliderOwnerTag;
        private readonly List<int> _collidersList = new List<int>(capacity: 4);

        [Inject]
        public ColliderOwnityApplier(CollidersTable collidersTable, World world) 
        {
            _collidersTable = collidersTable;
            _colliderOwnerTag = world.GetStash<RegisteredCollidersOwnerTag>();
        }

        public void CheckViewForColliders(Entity entity, IMonoView view)
        {
            if (view is ISingleColliderView singleColliderView)
            {
                ApplyOwnity(entity, singleColliderView);
            }
            else
            {
                if (view is IMultiColliderView multiColliderView)
                    ApplyOwnity(entity, multiColliderView);
            }
        }

        public void ApplyOwnity(Entity entity, ISingleColliderView view)
        {
            _collidersTable.RegisterCollider(entity, view.ColliderInstanceId);
            AddOwnerTag(entity);
        }

        public void ApplyOwnity(Entity entity, IMultiColliderView view)
        {
            view.FillCollidersList(_collidersList);
            foreach (var collider in _collidersList)
            {
                _collidersTable.RegisterCollider(entity, collider);
            }
            AddOwnerTag(entity);
        }

        private void AddOwnerTag(Entity entity) => _colliderOwnerTag.Set(entity);
    }
}
