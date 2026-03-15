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
        private NavigationMap _map;
        private CancellationTokenSource _tokenSource = new();

        [Inject]
        public NavigationMapInitializer(NavigationMap map) 
        {
            _map = map;            
        }

        public void OnAwake()
        {
            PrepareHexListCommand.Execute(_map);
            CalculateMapNavigation(_tokenSource.Token);            
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }

        private async void CalculateMapNavigation(CancellationToken token)
        {
            foreach (var hex in _map.Hexes)
            {
                 //await SetupHexFlowMapsCommand.ExecuteAsync(hex, _map.Settings);
            }

            _map.OnInitialized();
        }
    }
}
