using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using VContainer;
using Scellecs.Morpeh;
using ZE.MechBattle.Navigation;


namespace ZE.MechBattle.Ecs
{
    public class NavigationMapInitializer : IInitializer
    {
        public World World { get;set; }
        private readonly INavigationMap _map;
        private readonly HexRaycastRequestsList _hexRaycastRequests;
        private readonly CancellationTokenSource _cts = new();

        [Inject]
        public NavigationMapInitializer(INavigationMap map, HexRaycastRequestsList hexRaycastRequestsList) 
        {
            _map = map;     
            _hexRaycastRequests = hexRaycastRequestsList;
        }

        public void OnAwake()
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
            do
            {
                await Awaitable.NextFrameAsync();
            }
            while (!token.IsCancellationRequested && _hexRaycastRequests.Count != 0);

            if (token.IsCancellationRequested)
                return;

            _map.OnInitialized();
        }
    }
}
