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
        private readonly IUpdatableMap _map;
        private readonly MapSettingsSO _mapSettings;
        private readonly CancellationTokenSource _tokenSource = new();

        [Inject]
        public NavigationMapInitializer(IUpdatableMap map, MapSettingsSO settings) 
        {
            _map = map;            
            _mapSettings = settings;
        }

        public void OnAwake()
        {
            PrepareNavigationMapCommand.Execute(_map);
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
            _tokenSource.Dispose();
        }
    }
}
