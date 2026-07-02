using UnityEngine;
using ZE.Utils;
using ZE.MechBattle.Navigation;

namespace ZE.MechBattle
{
    public class MultipointSpawnHandler
    {
        private readonly INavigationMap _map;
        private readonly ShrinkingList<IntTriangularPos> _positionsList = new();
    
        public MultipointSpawnHandler(INavigationMap map)
        {
            _map = map;
        }

        public void Handle(ISpawner spawner, IntTriangularPos spawnerPos)
        {
            // todo: triangle radius enumerator
        }
    }
}
