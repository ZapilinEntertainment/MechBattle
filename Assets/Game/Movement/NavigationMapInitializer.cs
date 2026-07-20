using System;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZE.MechBattle.Navigation;


namespace ZE.MechBattle.Ecs
{
    public class NavigationMapInitializer : IInitializable, IDisposable
    {
        private readonly INavigationMap _map;
        private readonly HexRaycastRequestsList _hexRaycastRequests;
        private readonly CancellationTokenSource _cts = new();

        [Inject]
        public NavigationMapInitializer(INavigationMap map, HexRaycastRequestsList hexRaycastRequestsList) 
        {
            _map = map;     
            _hexRaycastRequests = hexRaycastRequestsList;
        }

        public void Initialize()
        {
            using var hexes = GetHexCoordsInRectangleCommand.Execute(_map.Settings, Allocator.Temp);
            foreach (var hexCoord in hexes)
            {
                var hex = _map.GetOrCreateHex(hexCoord);
                _hexRaycastRequests.AddRequest(hexCoord, hex.PassabilityVersion);
            }

            WaitUntilAllMapRaycasted();
        }

        public void Dispose() 
        { 
            _cts.Cancel();
            _cts.Dispose();
        }

        private async void WaitUntilAllMapRaycasted()
        {
            var token = _cts.Token;
            var allRequestsExecuted = false;
            do
            {
                await Awaitable.NextFrameAsync();
                allRequestsExecuted = _hexRaycastRequests.AwaitingCount == 0 && _hexRaycastRequests.CalculatingCount == 0;
                //UnityEngine.Debug.Log($"awaiting: {_hexRaycastRequests.AwaitingCount}, calculating: {_hexRaycastRequests.CalculatingCount}");
            }
            while (!(token.IsCancellationRequested | allRequestsExecuted));

            if (token.IsCancellationRequested)
                return;

            _map.OnInitialized();
            UnityEngine.Debug.Log("MAP CALCULATED");
        }

        
    }
}
