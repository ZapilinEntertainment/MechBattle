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
        private readonly MapSettingsSO _mapSettings;
        private readonly CancellationTokenSource _tokenSource = new();

        [Inject]
        public NavigationMapInitializer(INavigationMap map, MapSettingsSO settings) 
        {
            _map = map;            
            _mapSettings = settings;
        }

        public void OnAwake()
        {
            using (var hexList = GetHexesInRectangleCommand.Execute(_mapSettings, Allocator.Temp))
            {
                foreach (var hex in hexList)
                {
                    _map.AddHex(hex);
                }
            }

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
