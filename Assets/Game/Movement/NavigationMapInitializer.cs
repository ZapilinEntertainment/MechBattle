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

        [Inject]
        public NavigationMapInitializer(INavigationMap map, HexRaycastRequestsList hexRaycastRequestsList) 
        {
            _map = map;     
            _hexRaycastRequests = hexRaycastRequestsList;
        }

        public void OnAwake()
        {
            using var hexes = GetHexesInRectangleCommand.Execute(_map.Settings, Allocator.Temp);
            foreach (var hex in hexes)
            {
                _hexRaycastRequests.Add(hex);
            }
        }

        public void Dispose() { }
    }
}
