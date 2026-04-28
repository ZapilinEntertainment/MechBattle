using System;
using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Navigation
{
    public class NavigationMapController : IDisposable
    {
        private readonly NavigationMap _map;

        public NavigationMapController(NavigationMap map)
        {
            _map = map;
        }

        public void Dispose()
        {
            _map.Dispose();
        }
    }
}
