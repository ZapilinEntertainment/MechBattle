using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Scellecs.Morpeh;
using R3;

namespace ZE.MechBattle.Ecs {

    // create damageable entities from scene decorations
    // (using searching to avoid direct inject in every damageable decoration GO)
    public sealed class DamageablesInitializer : IInitializable
    {
        private Stash<HealthComponent> _healthComponents;
        private Stash<RegisteredCollidersOwnerTag> _collidersOwnerTag; 
        private Stash<ViewDestroyEffectComponent> _viewDestroyEffect;

        private readonly World _world;
        private readonly CollidersTable _collidersTable;
        private readonly StringDataDictionary _stringDictionary;
        private readonly ViewSynchronizationApplier _viewSyncApplier;

        [Inject]
        public DamageablesInitializer(
            World world,
            CollidersTable collidersTable, 
            StringDataDictionary strDict, 
            ViewSynchronizationApplier viewSyncApplier)
        {
            _world = world;
            _collidersTable = collidersTable;
            _stringDictionary = strDict;
            _viewSyncApplier = viewSyncApplier;
        }

        public void Initialize()
        {
            _healthComponents = _world.GetStash<HealthComponent>();
            _collidersOwnerTag = _world.GetStash<RegisteredCollidersOwnerTag>();
            _viewDestroyEffect = _world.GetStash<ViewDestroyEffectComponent>();

            var destructibleDecorations = GameObject.FindObjectsByType<DestructibleDecoration>(FindObjectsSortMode.None);
            foreach (var decoration in destructibleDecorations)
            {
                CreateDamageableEntity(decoration);
            }
        }

        private void CreateDamageableEntity(IDamageableView view)
        {
            var entity = _world.CreateEntity();
            _viewSyncApplier.Apply(entity, view);

            var parameters = view.GetParameters();
            _healthComponents.Set(entity, new() { CurrentValue = parameters.Health, MaxValue = parameters.Health});           
            
            var colliderIds = view.GetColliderIds();
            foreach (var id in colliderIds)
                _collidersTable.RegisterCollider(entity, id);
            // colliders will be cleared from list by CollidersClearSystem
            _collidersOwnerTag.Set(entity, new());

            var destroyEffectKey = view.ViewDestroyEffectKey;
            if (!string.IsNullOrEmpty(destroyEffectKey))
            {
                var encodedKey = _stringDictionary.StringToKey(destroyEffectKey);
                _viewDestroyEffect.Set(entity, new() { EffectKey = encodedKey });
            }               
        }

     
    }
}