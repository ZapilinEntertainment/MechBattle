using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct InitialDelayComponent : IComponent 
    {
        public readonly float StopTime;    

        public InitialDelayComponent(float stopTime)
        {
            StopTime = stopTime;
        }
    }
}