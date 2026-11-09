using UnityEngine;
using TriInspector;
using VContainer;
using Unity.Mathematics;
using ZE.MechBattle.Ecs;
using System;
using Scellecs.Morpeh;
using System.Collections.Generic;

namespace ZE.MechBattle.Develop
{
    public class VirtualViewCreateUtility : MonoBehaviour
    {
        [Serializable]
        private class UtilityConfig : IVirtualViewConfig
        {
            [field:SerializeField] public string MeshKey { get; private set; }
            [field: SerializeField] public string MaterialKey { get; private set; }
            [field: SerializeField] public float3 Scale { get; private set; }
            [field: SerializeField] public float VisibilityLimit { get; private set; }
        }

        [SerializeField] private UtilityConfig _config;
        [SerializeField] private float _visibilityLimit = 0f;
        private bool _isInjected = false;
        private VirtualViewFactory _factory;
        private TransformAspectHandler _transformAspectHandler;
        private List<Entity> _createdEntities = new();

        [Inject]
        public void Inject(VirtualViewFactory virtualViewFactory, TransformAspectHandler transformAspectHandler)
        {
            _isInjected = true;
            _factory = virtualViewFactory;
            _transformAspectHandler = transformAspectHandler;
        }

        [EnableInPlayMode, Button("Spawn")]
        public void Spawn()
        {
            if (!_isInjected)
                return;

            var entity = _factory.CreateViewEntity(_config);
            _transformAspectHandler.SetPosition(entity, transform.position);
            _transformAspectHandler.SetRotation(entity, transform.rotation);
            _createdEntities.Add(entity);
        }
    }
}
