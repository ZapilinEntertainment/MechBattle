using System;
using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public struct ViewComponent : IComponent, IDisposable 
    {
        public IDisposable Value;

        public void Dispose() 
        {
            if (Value != null)
            {
                Value.Dispose();
                Value = null;
            }            
        }
    }
}