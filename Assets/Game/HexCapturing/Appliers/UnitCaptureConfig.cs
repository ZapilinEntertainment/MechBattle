using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [Serializable]
    public class UnitCaptureConfig : IUnitFeatureConfig
    {
        [field: SerializeField] public CaptureBehaviour CaptureBehaviour { get; private set; }    
    }
}
