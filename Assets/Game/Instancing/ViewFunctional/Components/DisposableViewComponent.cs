using System;
using Scellecs.Morpeh;
using UnityEngine;
using Unity.IL2CPP.CompilerServices;

namespace ZE.MechBattle.Ecs {
    [System.Serializable]
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public readonly struct DisposableViewComponent : IComponent, IDisposable 
    {
        private readonly GameObject _view;

        public DisposableViewComponent(GameObject gameObject)
        {
            _view = gameObject;
        }

        public void Dispose()
        {
            GameObject.Destroy(_view);
        }

    }
}