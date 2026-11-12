using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle.Views
{
    public class VirtualViewApplier
    {
        private readonly VirtualViewConfigsList _configsList;
        private readonly MeshesDepot _meshesDepot;
        private readonly MaterialsDepot _materialsDepot;
        private readonly Stash<VirtualViewComponent> _virtualViews;
        private readonly Dictionary<string, VirtualViewComponent> _viewKeyToComponent = new();

        [Inject]
        public VirtualViewApplier(World world, VirtualViewConfigsList configsList, MeshesDepot meshesDepot, MaterialsDepot materialsDepot)
        {
            _configsList = configsList;
            _meshesDepot = meshesDepot;
            _materialsDepot = materialsDepot;

            _virtualViews = world.GetStash<VirtualViewComponent>();
        }

        public void ApplyRawViewId(Entity entity, string rawViewId)
        {
            // note: view id here looks like "virtual:ViewId"
            // yes, duplicating positions for caching (virtual:X and X)

            if (!_viewKeyToComponent.TryGetValue(rawViewId, out var component))
            {
                var realId = rawViewId.Substring(ViewConstants.VIRTUAL_VIEW_PREFIX.Length);
                if (!TryLoadComponentFromConfig(realId, out component))
                    return;

                _viewKeyToComponent.Add(rawViewId, component);
            }

            Apply(entity, component);
        }

        public void Apply(Entity entity, string viewConfigId)
        {
            if (!_viewKeyToComponent.TryGetValue(viewConfigId, out var component))
            {
                if (!TryLoadComponentFromConfig(viewConfigId, out component))
                    return;

                _viewKeyToComponent.Add(viewConfigId, component);
            }
            Apply(entity, component);
        }

        public void Apply(Entity entity, VirtualViewSetupData data) =>
            Apply(entity, ConfigToComponent(data));

        public void Apply(Entity entity, VirtualViewComponent viewComponent)
        {
            _virtualViews.Add(entity, viewComponent);
        }

        private bool TryLoadComponentFromConfig(string key, out VirtualViewComponent component)
        {
            if (!_configsList.TryGetElement(key, out var config))
            {
                UnityEngine.Debug.Log("cannot find config " + key);
                component = default;
                return false;
            }

            component = ConfigToComponent(config);
            return true;
        }

        private VirtualViewComponent ConfigToComponent(VirtualViewSetupData data)
        {
            var meshId = -1;
            if (!string.IsNullOrEmpty(data.MeshKey))
                _meshesDepot.TryGetElementIndex(data.MeshKey, out meshId);

            var materialId = -1;
            if (!string.IsNullOrEmpty(data.MaterialKey))
                _materialsDepot.TryGetElementIndex(data.MaterialKey, out materialId);

            return new(meshId, data.SubmeshIndex, materialId, data.Scale, data.VisibilityLimit);
        }
    }
}
