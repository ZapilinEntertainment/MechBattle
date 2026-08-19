using System;
using UnityEngine;

namespace ZE.MechBattle
{
    public interface IWeaponRayCaster : IDisposable
    {
        float MaxCastDistance { get; }
        void UpdateFrameIndex(int frameIndex);
        void UpdateEndPoints(Vector3 start, Vector3 end, bool hit);
        float CalculateCurrentDamageCf();
        bool IsOutdated(int currentFrameIndex);
    
    }
}
