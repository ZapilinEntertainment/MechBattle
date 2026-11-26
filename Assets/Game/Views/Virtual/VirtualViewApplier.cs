using Scellecs.Morpeh;
using System.Collections.Generic;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class VirtualViewApplier : IViewApplyStrategy
    {
        public readonly struct VirtualViewApplyData
        {
            public readonly VirtualViewComponent ViewComponent;
            public readonly LocalPositionComponent LocalPositionComponent;
            public readonly LocalRotationComponent LocalRotationComponent;

            public readonly bool ContainFactionColorMaterial;
            public float3 LocalPosition => LocalPositionComponent.Value;
            public quaternion LocalRotation => LocalRotationComponent.Value;

            public VirtualViewApplyData(VirtualViewComponent viewComponent, LocalPositionComponent localPosition, LocalRotationComponent localRotation, bool containFactionColorMaterial)
            {
                ViewComponent = viewComponent;
                LocalPositionComponent = localPosition;
                LocalRotationComponent = localRotation;
                ContainFactionColorMaterial = containFactionColorMaterial;
            }

            public VirtualViewApplyData(VirtualViewComponent viewComponent, float3 localPosition, quaternion localRotation, bool containFactionColorMaterial)
            {
                ViewComponent = viewComponent;
                LocalPositionComponent = new() { Value = localPosition};
                LocalRotationComponent = new() { Value = localRotation};
                ContainFactionColorMaterial = containFactionColorMaterial;
            }
        }

        private readonly VirtualViewConfigsList _configsList;
        private readonly MeshesDepot _meshesDepot;
        private readonly MaterialsDepot _materialsDepot;
        private readonly Stash<VirtualViewComponent> _virtualViews;
        private readonly Stash<ViewContainerComponent> _viewContainers;
        private readonly Stash<LocalPositionComponent> _localPositions;
        private readonly Stash<LocalRotationComponent> _localRotations;
        private readonly Stash<FactionColorApplicableTag> _factionColorTags;
        private readonly Dictionary<string, VirtualViewApplyData> _viewKeyToApplyData = new();
        private readonly IViewContainersManager _viewContainersManager;
        public ViewMode RealizingViewType => ViewMode.Virtual;

        [Inject]
        public VirtualViewApplier(
            World world,
            VirtualViewConfigsList configsList,
            MeshesDepot meshesDepot,
            MaterialsDepot materialsDepot,
            IViewContainersManager viewContainers)
        {
            _configsList = configsList;
            _meshesDepot = meshesDepot;
            _materialsDepot = materialsDepot;
            _viewContainersManager = viewContainers;

            _virtualViews = world.GetStash<VirtualViewComponent>();
            _viewContainers = world.GetStash<ViewContainerComponent>();
            _localPositions = world.GetStash<LocalPositionComponent>();
            _localRotations = world.GetStash<LocalRotationComponent>();
            _factionColorTags = world.GetStash<FactionColorApplicableTag>();
        }

        public void ApplyView(Entity entity, string viewConfigId)
        {
            if (!_viewKeyToApplyData.TryGetValue(viewConfigId, out var applyData))
            {
                if (!TryLoadComponentFromConfig(viewConfigId, out applyData))
                    return;

                _viewKeyToApplyData.Add(viewConfigId, applyData);
            }
            Apply(entity, applyData);
        }

        public void Apply(Entity entity, VirtualViewSetupData data) =>
            Apply(entity, ConfigToApplyData(data));

        public void Apply(Entity entity, VirtualViewApplyData applyData)
        {
            _virtualViews.Add(entity, applyData.ViewComponent);
            _localPositions.Set(entity, applyData.LocalPositionComponent);
            _localRotations.Set(entity, applyData.LocalRotationComponent);
            if (applyData.ContainFactionColorMaterial)
            {
                _factionColorTags.Set(entity);
            }
                
        }

        private bool TryLoadComponentFromConfig(string key, out VirtualViewApplyData applyData)
        {
            if (!_configsList.TryGetElement(key, out var config))
            {
                UnityEngine.Debug.Log("cannot find config " + key);
                applyData = default;
                return false;
            }

            applyData = ConfigToApplyData(config);
            return true;
        }

        public VirtualViewApplyData ConfigToApplyData(VirtualViewSetupData data)
        {
            var meshId = -1;
            if (!string.IsNullOrEmpty(data.MeshKey))
                _meshesDepot.TryGetElementIndex(data.MeshKey, out meshId);

            var materialId = -1;
            var containsFactionColorMaterial = false;
            if (!string.IsNullOrEmpty(data.MaterialSetupData.MaterialKey))
            {
                _materialsDepot.TryGetElementIndex(data.MaterialSetupData.MaterialKey, out materialId);
                containsFactionColorMaterial = data.MaterialSetupData.ApplyFactionColor;
            }
                

            var localPosition = new LocalPositionComponent() { Value = data.Position };
            var localRotation = new LocalRotationComponent() { Value = data.Rotation };

            return new (
                new VirtualViewComponent(meshId, data.SubmeshIndex, materialId, data.Scale, data.VisibilityLimit),
                localPosition,
                localRotation,
                containsFactionColorMaterial);
        }

        public IViewContainer AddEmptyViewContainer(Entity entity)
        {
            var (container, id) = _viewContainersManager.GetVirtualContainer();
            _viewContainers.Set(entity, new(id));
            return container;
        }
    }
}
