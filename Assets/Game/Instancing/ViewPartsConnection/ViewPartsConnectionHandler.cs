using Scellecs.Morpeh;
using Unity.Mathematics;
using VContainer;
using ZE.MechBattle.Ecs;

namespace ZE.MechBattle
{
    // why co complicated approach instead of transform.parent =transform ?
    // a) there is possible situation where we need to connect viewless entity to transform, transform to gpu-drawn entities, component to entity and etc
    // b) most of operation object are not defined as mono-objects, but some interfaces instead
    public class ViewPartsConnectionHandler
    {
        private readonly IViewContainersPool _viewContainersPool;
        private readonly ViewPartConnectionsList _connectionsList;
        private readonly Stash<ViewPartConnectedTag> _viewPartsConnectedTag;
        private readonly Stash<ViewContainerComponent> _viewContainers;

        [Inject]
        public ViewPartsConnectionHandler(World world, IViewContainersPool viewContainersPool, ViewPartConnectionsList viewPartConnectionsList)
        {
            _viewContainersPool = viewContainersPool;
            _connectionsList = viewPartConnectionsList;

            _viewPartsConnectedTag = world.GetStash<ViewPartConnectedTag>();
            _viewContainers = world.GetStash<ViewContainerComponent>();
        }

        public void Connect(Entity parentPointEntity, IConnectableViewPart viewPart, float3 localPos, quaternion localRot)
        {
            var viewContainerId = _viewContainers.Get(parentPointEntity).Id;
            if (!_viewContainersPool.TryGetContainer(viewContainerId, out var container))
                throw new System.Exception("cannot connect: no view container set");

            if (container is not IViewConnectionsPoint connectionPoint)
                throw new System.Exception("cannot connect: view container is not IConnectionPoint");

            if (connectionPoint is IMonoView pointMono)
            {
                if (viewPart is IMonoView partMono)
                {
                    partMono.SetParent(pointMono.Transform);
                    partMono.Transform.SetLocalPositionAndRotation(localPos, localRot);
                    _connectionsList.OnConnected(connectionPoint, viewPart);
                    _viewPartsConnectedTag.Set(parentPointEntity);
                    return;
                }
            }

            UnityEngine.Debug.Log("view parts connection error");
        }
    
    }
}
