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

        [Inject]
        public NavigationMapInitializer(NavigationMap map) 
        {
            InitializeMapHexesCommand.Execute(map);
        }

        public void OnAwake()
        {
            
        }

        public void Dispose()
        {
        }
    }
}
