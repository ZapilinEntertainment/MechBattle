using UnityEngine;
using TriInspector;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using System;
using Scellecs.Morpeh;
using System.Collections.Generic;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Develop
{
    public class VirtualViewCreateUtility : MonoBehaviour
    {

        [SerializeField] private VirtualViewSetupData _config;
        [SerializeField] private float _visibilityLimit = 0f;
        private bool _isInjected = false;
        private VirtualViewApplier _virtualViewApplier;
        private TransformAspectHandler _transformAspectHandler;
        private World _world;
        private List<Entity> _createdEntities = new();

        [Inject]
        public void Inject(VirtualViewApplier virtualViewApplier, TransformAspectHandler transformAspectHandler, World world)
        {
            _isInjected = true;
            _virtualViewApplier = virtualViewApplier;
            _transformAspectHandler = transformAspectHandler;
            _world = world;
        }

        [EnableInPlayMode, Button("Spawn")]
        public void Spawn()
        {
            if (!_isInjected)
                return;

            var entity = _world.CreateEntity();
            _virtualViewApplier.Apply(entity, _config);
            _transformAspectHandler.SetPosition(entity, _config.Position);
            _transformAspectHandler.SetRotation(entity, _config.Rotation);
            _createdEntities.Add(entity);
        }

        [EnableInPlayMode, Button("Clear")]
        public void Clear()
        {
            if (_createdEntities.Count == 0)
                return;
            foreach (var entity in _createdEntities)
            {
                _world.RemoveEntity(entity);
            }
            _createdEntities.Clear();
        }
    }
}
