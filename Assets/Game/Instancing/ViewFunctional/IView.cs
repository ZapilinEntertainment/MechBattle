using System;
using UnityEngine;

namespace ZE.MechBattle.Views
{
    public interface IView : IDisposable
    {
        void SetParent(Transform parent);
    
    }
}
