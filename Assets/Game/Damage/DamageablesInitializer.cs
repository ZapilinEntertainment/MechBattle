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
        private readonly StringDataDictionary _stringDictionary;
        private readonly ViewSynchronizationApplier _viewSyncApplier;

        [Inject]
        public DamageablesInitializer(
            World world,
            StringDataDictionary strDict, 
            ViewSynchronizationApplier viewSyncApplier)
        {
            _world = world;
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

        // TODO: rework to damageables factory
        private void CreateDamageableEntity(IDamageableView view)
        {
            var entity = _world.CreateEntity();
            _viewSyncApplier.Apply(entity, view, applyViewPosition: true);

            var parameters = view.GetParameters();
            _healthComponents.Set(entity, new(parameters.Health));           


            var destroyEffectKey = view.ViewDestroyEffectKey;
            if (!string.IsNullOrEmpty(destroyEffectKey))
            {
                var encodedKey = _stringDictionary.StringToKey(destroyEffectKey);
                _viewDestroyEffect.Set(entity, new() { EffectKey = encodedKey });
            }               
        }

     
    }
}