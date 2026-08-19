using System;
using UnityEngine;

namespace ZE.MechBattle
{
    public interface IRayEffectView
    {
        Vector3 Start { get; set; }
        Vector3 End { get; set; }
        void SetEndEffectActivity(bool isVisible);    
    }

    public interface IDisposableRayEffectView : IRayEffectView, IDisposable { }
}
