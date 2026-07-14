using System;
using System.Collections.Generic;
using VContainer;
using Scellecs.Morpeh;
using Unity.Mathematics;
using Unity.IL2CPP.CompilerServices;
using ZE.MechBattle.Views;

namespace ZE.MechBattle.Ecs
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]

    // reading view requests and launch loading processes
    // why so complicated - host should not freeze when loading own client views
    // also there can be some visibility restrictions on request-calling systems
    // (we can easily unload and load poolable views, but left view containers untouched)
    public sealed class ViewRequestsHandleSystem : ISystem
    {
        private struct ViewRequest
        {
            public Entity ReceiveEntity;
            public IViewProvider Provider;
        }

        public World World { get; set; }
        private Filter _requestsFilter;
        private Stash<ViewLoadRequestTag> _requests;
        private Stash<ViewKeyComponent> _viewKeys;
        private Stash<ViewContainerComponent> _viewContainerComponents;

        private readonly ViewProviderFactory _viewProviderFactory;
        private readonly IViewContainersPool _viewContainersList;
        private readonly List<ViewRequest> _executableRequests = new();

        [Inject]
        public ViewRequestsHandleSystem(ViewProviderFactory viewProviderFactory, IViewContainersPool viewContainersList)
        {
            _viewProviderFactory = viewProviderFactory;
            _viewContainersList = viewContainersList;
        }

        public void OnAwake()
        {
            _requestsFilter = World.Filter.With<ViewLoadRequestTag>().Without<EntityDisposeTag>().Build();

            _requests = World.GetStash<ViewLoadRequestTag>();
            _viewKeys = World.GetStash<ViewKeyComponent>();
            _viewContainerComponents = World.GetStash<ViewContainerComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_requestsFilter.IsNotEmpty())
            {
                foreach (var entity in _requestsFilter)
                {
                    var viewKey = _viewKeys.Get(entity).Value;
                    var provider = _viewProviderFactory.GetViewProvider(viewKey);
                    if (provider.IsReadyToProvide)
                    {
                        _executableRequests.Add(new()
                        {
                            ReceiveEntity = entity,
                            Provider = provider,
                        });
                    }                       
                }

                var requestsCount = _executableRequests.Count;
                if (requestsCount == 0)
                    return;

                // TODO: there can be more complicated logic of loading cost
                requestsCount = math.min(requestsCount, GameConstants.MAX_VIEW_INSTANTIATIONS_PER_FRAME);
                for (var i = 0; i < requestsCount; i++) 
                {
                    var request = _executableRequests[i];                   
                    ExecuteRequest(request);
                }

                _executableRequests.Clear();
            }
        }

        public void Dispose()
        {
            _executableRequests.Clear();
        }

        private void ExecuteRequest(ViewRequest request)
        {
            _requests.Remove(request.ReceiveEntity);

            var containerId = _viewContainerComponents.Get(request.ReceiveEntity).Id;
            if (!_viewContainersList.TryGetContainer(containerId, out var viewContainer))
            {
                #if UNITY_EDITOR
                UnityEngine.Debug.LogWarning($"no container found by id {containerId}");
                #endif
                return;
            }

            viewContainer.OnViewInstanced(request.Provider.GetView());
        }
    }
}