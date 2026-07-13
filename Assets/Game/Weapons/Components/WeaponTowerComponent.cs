using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;
using TriInspector;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct WeaponTowerComponent : IComponent 
    {
        public readonly Entity TowerEntity;
        #if UNITY_EDITOR
        [ShowInInspector] public Entity towerEntity => TowerEntity;
        #endif

        public WeaponTowerComponent(Entity towerEntity) => TowerEntity = towerEntity;
    }
}