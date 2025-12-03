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
    // also there can be some visibility restrictions on request-calling systems (there will be a GO, but without real view)
    public sealed class ViewRequestsHandleSystem : ISystem
    {
        private struct ViewRequest
        {
            public Entity Entity;
            public IViewProvider Provider;
            public int ViewReceiverKey;
        }

        public World World { get; set; }
        private Filter _requestsFilter;
        private Stash<ViewRequestComponent> _requests;
        private Stash<ViewInfoComponent> _viewInfos;

        private readonly ViewProviderFactory _viewProviderFactory;
        private readonly ViewReceiversList _viewReceivers;
        private readonly List<ViewRequest> _executableRequests = new();

        [Inject]
        public ViewRequestsHandleSystem(ViewProviderFactory viewProviderFactory, ViewReceiversList viewReceivers)
        {
            _viewProviderFactory = viewProviderFactory;
            _viewReceivers = viewReceivers;
        }

        public void OnAwake()
        {
            _requestsFilter = World.Filter.With<ViewRequestComponent>().Build();

            _requests = World.GetStash<ViewRequestComponent>();
            _viewInfos = World.GetStash<ViewInfoComponent>();
        }

        public void OnUpdate(float deltaTime)
        {
            if (_requestsFilter.IsNotEmpty())
            {
                foreach (var entity in _requestsFilter)
                {
                    var viewKey = _viewInfos.Get(entity).Value;
                    var provider = _viewProviderFactory.GetViewProvider(viewKey);
                    if (provider.IsReadyToProvide)
                    {
                        _executableRequests.Add(new()
                        {
                            Entity = entity,
                            Provider = provider,
                            ViewReceiverKey = _requests.Get(entity).ReceiverId
                        });
                    }                       
                }

                var requestsCount = _executableRequests.Count;
                if (requestsCount == 0)
                    return;

                // TODO: there can be more complicated logic of loading cost
                requestsCount = math.min(requestsCount, GameConstants.MAX_INSTANCE_PER_FRAME);
                for (var i = 0; i < requestsCount; i++) 
                {
                    var request = _executableRequests[i];
                    _requests.Remove(request.Entity);

                    if (!_viewReceivers.TryGetElement(request.ViewReceiverKey, out var receiver))
                        continue;

                    receiver.OnViewLoaded(request.Provider.GetView());

                    _viewReceivers.Unregister(request.ViewReceiverKey);
                }

                _executableRequests.Clear();
            }
        }

        public void Dispose()
        {
            _executableRequests.Clear();
        }
    }
}