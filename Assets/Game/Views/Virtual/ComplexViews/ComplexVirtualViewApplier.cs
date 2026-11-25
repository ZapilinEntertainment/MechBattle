using System;
using System.Collections.Generic;
using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class ComplexVirtualViewApplier : IViewApplyStrategy
    {
        public ViewMode RealizingViewType => ViewMode.ComplexVirtual;
        private readonly IViewContainersManager _containersManager;
        private readonly Stash<ViewPartKeyComponent> _viewPartKeys;
        private readonly Stash<ViewContainerComponent> _viewContainers;      
        private readonly ComplexVirtualViewConfigsList _complexConfigs;
        private readonly VirtualViewApplier _virtualViewApplier;
        private readonly Dictionary<string, ComplexViewPreset> _presets = new(4);
        private readonly World _world;
        private readonly ParentingRelationsApplier _parentingRelationsApplier;

        [Inject]
        public ComplexVirtualViewApplier(
            IViewContainersManager containersManager, 
            World world, 
            ComplexVirtualViewConfigsList configsList,
            VirtualViewApplier virtualViewApplier,
            ParentingRelationsApplier parentingRelationsApplier)
        {
            _containersManager = containersManager;
            _complexConfigs = configsList;
            _virtualViewApplier = virtualViewApplier;
            _parentingRelationsApplier = parentingRelationsApplier;
            _world = world;

            _viewPartKeys = _world.GetStash<ViewPartKeyComponent>();
            _viewContainers = _world.GetStash<ViewContainerComponent>();            
        }

        public IViewContainer AddEmptyViewContainer(Entity entity) => _containersManager.GetVirtualContainer().container;

        public void ApplyView(Entity baseEntity, string viewId)
        {
            if (!_complexConfigs.TryGetValue(viewId, out var config))
            {
                UnityEngine.Debug.LogError("complex config not found: " + viewId);
                return;
            }

            // get or create already prepared data
            var viewContainerData = _containersManager.GetVirtualContainer();
            _viewContainers.Set(baseEntity, new(viewContainerData.id));

            if (!_presets.TryGetValue(viewId, out var preset))
            {
                preset = new ComplexViewPreset(config, _virtualViewApplier);
                _presets.Add(viewId, preset);
            }

            // create part entities
            var count = preset.SortedData.Length;
            Span<Entity> entities = stackalloc Entity[preset.SortedData.Length];
            for (var i = 0; i < count;i++)
            {
                var data = preset.SortedData[i];
                var parentEntity = data.ParentKey.IsValid ? entities[data.ParentIndex] : baseEntity;
                entities[i] = AddPartView(parentEntity, data.PartKey, data.ApplyData, viewContainerData.container);
            }
        }

        private Entity AddPartView(Entity parentEntity, ViewPartKey viewPartKey, VirtualViewApplier.VirtualViewApplyData applyData, VirtualViewContainer container)
        {
            var partEntity = _world.CreateEntity();
            _virtualViewApplier.Apply(partEntity, applyData);

            if (viewPartKey.IsValid)
                _viewPartKeys.Add(partEntity, new(viewPartKey));

            _parentingRelationsApplier.Apply(new()
            {
                ParentEntity = parentEntity,
                ChildEntity = partEntity,
                LocalPos = applyData.LocalPosition,
                LocalRot = applyData.LocalRotation,
            });

            container.AddPartEntity(partEntity);
            return partEntity;
        }

        public Entity AddPartView(
            Entity parentEntity,
            ViewPartKey viewPartKey,
            VirtualViewComponent viewData,
            float3 localPos,
            quaternion localRot,
            VirtualViewContainer container) =>
            AddPartView(parentEntity, viewPartKey, new VirtualViewApplier.VirtualViewApplyData(viewData, localPos, localRot, false), container);
    }
}
