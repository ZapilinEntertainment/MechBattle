using Scellecs.Morpeh;
using System.Collections.Generic;
using VContainer;
using ZE.MechBattle.Ecs;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    public class VirtualViewFactory : IViewInstanceStrategy
    {
        // note: for complex views - continue develop virtual view containers

        private readonly World _world;
        private readonly VirtualViewConfigsList _configsList;
        private readonly MeshesDepot _meshesDepot;
        private readonly MaterialsDepot _materialsDepot;
        private readonly Stash<VirtualViewComponent> _virtualViews;
        private readonly Dictionary<string, VirtualViewComponent> _viewKeyToComponent = new();

        [Inject]
        public VirtualViewFactory(World world, VirtualViewConfigsList configsList, MeshesDepot meshesDepot, MaterialsDepot materialsDepot)
        {
            _world = world;
            _configsList = configsList;
            _meshesDepot = meshesDepot;
            _materialsDepot = materialsDepot;

            _virtualViews = _world.GetStash<VirtualViewComponent>();
        }

        Entity IViewInstanceStrategy.CreateViewEntity(string viewId)
        {
            // note: view id here looks like "virtual:ViewId"
            // yes, duplicating positions for caching (virtual:X and X)

            if (!_viewKeyToComponent.TryGetValue(viewId, out var component))
            {
                var realId = viewId.Substring(ViewConstants.VIRTUAL_VIEW_PREFIX.Length);
                if (!TryLoadComponentFromConfig(realId, out component))
                    return default;

                _viewKeyToComponent.Add(viewId, component);
            }

            return CreateViewEntity(component);
        }

        public Entity CreateViewEntity(string viewConfigId)
        {
            if (!_viewKeyToComponent.TryGetValue(viewConfigId, out var component))
            {
                if (!TryLoadComponentFromConfig(viewConfigId, out component))
                    return default;

                _viewKeyToComponent.Add(viewConfigId, component);
            }
            return CreateViewEntity(component);
        }

        public Entity CreateViewEntity(IVirtualViewConfig config) => CreateViewEntity(ConfigToComponent(config));

        public Entity CreateViewEntity(VirtualViewComponent viewComponent)
        {
            var entity = _world.CreateEntity();
            _virtualViews.Add(entity, viewComponent);
            return entity;
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

        private VirtualViewComponent ConfigToComponent(IVirtualViewConfig config)
        {
            var meshId = -1;
            if (!string.IsNullOrEmpty(config.MeshKey))
                _meshesDepot.TryGetElementIndex(config.MeshKey, out meshId);

            var materialId = -1;
            if (!string.IsNullOrEmpty(config.MaterialKey))
                _materialsDepot.TryGetElementIndex(config.MaterialKey, out materialId);

            return new(meshId, materialId, config.Scale, config.VisibilityLimit);
        }
    }
}
