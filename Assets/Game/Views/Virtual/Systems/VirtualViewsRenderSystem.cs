using Scellecs.Morpeh;
using System;
using System.Collections.Generic;
using Unity.IL2CPP.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using VContainer;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs {
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed class VirtualViewsRenderSystem : ISystem 
    {

        public World World { get; set;}
        private Filter _filter;
        private Stash<VirtualViewComponent> _virtualViews;
        private readonly Dictionary<DrawInstanceGroupKey, List<Entity>> _groupedEntities = new();
        private readonly List<Matrix4x4> _matricesBuffer = new();
        private readonly TransformAspectHandler _transformAspectHandler;
        private readonly VirtualRenderer _virtualRenderer;

        [Inject]
        public VirtualViewsRenderSystem(TransformAspectHandler transformAspectHandler, VirtualRenderer virtualRenderer)
        {
            _transformAspectHandler = transformAspectHandler;
            _virtualRenderer = virtualRenderer;
        }

        public void OnAwake() 
        {
            _filter = World.Filter
                .With<VirtualViewComponent>()
                .With<VisibleVirtualViewTag>()
                .Without<EntityDisposeTag>()
                .Build();

            _virtualViews = World.GetStash<VirtualViewComponent>();
        }

        public void OnUpdate(float deltaTime) 
        {
            if (_filter.IsEmpty())
                return;

            foreach (var entity in _filter)
            {
                var viewComponent = _virtualViews.Get(entity);
                var viewKey = new DrawInstanceGroupKey(viewComponent.MaterialId, viewComponent.MeshId, viewComponent.SubmeshIndex);
                if (!_groupedEntities.TryGetValue(viewKey, out var list))
                {
                    list = new();
                    _groupedEntities.Add(viewKey, list);
                }

                list.Add(entity);
            }

            foreach (var kvp in _groupedEntities)
            {
                foreach (var element in kvp.Value)
                {
                    var scale = _virtualViews.Get(element).Scale;
                    var point = _transformAspectHandler.GetPoint(element);
                    _matricesBuffer.Add(Matrix4x4.TRS(point.pos, point.rot, scale));
                }
                _virtualRenderer.DrawMatrices(kvp.Key, _matricesBuffer);
                _matricesBuffer.Clear();
            }

            foreach (var list in _groupedEntities.Values)
            {
                list.Clear();
            }
        }

        public void Dispose()
        {

        }
    }
}