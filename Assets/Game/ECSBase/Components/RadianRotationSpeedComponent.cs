using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct RotationSpeedComponent : IComponent 
    {
        public float RadianValue => _radianValue;
        private readonly float _radianValue;
        
        public RotationSpeedComponent(float radianValue)
        {
            _radianValue = radianValue;
        }
    }
}