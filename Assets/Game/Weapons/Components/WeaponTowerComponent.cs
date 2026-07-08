using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct WeaponTowerComponent : IComponent 
    {
        public float RotationRadianValue;
        public readonly float RadianRotationSpeed;

        public WeaponTowerComponent(float rotationSpeed)
        {
            RadianRotationSpeed = rotationSpeed;
            RotationRadianValue = 0f;
        }
    }
}