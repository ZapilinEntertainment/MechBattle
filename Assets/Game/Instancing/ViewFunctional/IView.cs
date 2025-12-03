using System;
using UnityEngine;

namespace ZE.MechBattle
{
    public interface IView : IDisposable
    {
        void SetParent(Transform parent);
    
    }
}
