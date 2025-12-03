using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using Scellecs.Morpeh;
using Unity.IL2CPP.CompilerServices;
using R3;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // create damageable entities from scene decorations
    // (using searching to avoid direct inject in every damageable decoration GO)
    public sealed class DamageablesInitializer : IInitializer 
    {
        public World World { get; set;}
        private Stash<HealthComponent> _healthComponents;
        private Stash<RegisteredCollidersOwnerTag> _collidersOwnerTag; 
        private Stash<ViewDestroyEffectComponent> _viewDestroyEffect;
        private readonly CollidersTable _collidersTable;
        private readonly StringDataDictionary _stringDictionary;
        private readonly EntityFactory _entityFactory;

        [Inject]
        public DamageablesInitializer(CollidersTable collidersTable, StringDataDictionary strDict, EntityFactory entityBuilder)
        {
            _collidersTable = collidersTable;
            _stringDictionary = strDict;
            _entityFactory = entityBuilder;
        }

        public void OnAwake() 
        {
            _healthComponents = World.GetStash<HealthComponent>();
            _collidersOwnerTag = World.GetStash<RegisteredCollidersOwnerTag>();
            _viewDestroyEffect = World.GetStash<ViewDestroyEffectComponent>();

            var destructibleDecorations = GameObject.FindObjectsByType<DestructibleDecoration>(FindObjectsSortMode.None);
            foreach (var decoration in destructibleDecorations)
            {
                CreateDamageableEntity(decoration);
            }
        }


        public void Dispose() { }

        private void CreateDamageableEntity(IDamageableView view)
        {
            var entity = _entityFactory.Build(view);

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
                var encodedKey = _stringDictionary.GetStringKey(destroyEffectKey);
                _viewDestroyEffect.Set(entity, new() { EffectKey = encodedKey });
            }               
        }
    }
}