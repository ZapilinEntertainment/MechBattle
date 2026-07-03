using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct InitialDelayComponent : IDelayComponent 
    {
        public float StopTime => _stopTime;
        private readonly float _stopTime;    

        public InitialDelayComponent(float stopTime)
        {
            _stopTime = stopTime;
        }
    }
}