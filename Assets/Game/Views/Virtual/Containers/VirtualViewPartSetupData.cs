using System;
using UnityEngine;
using ZE.MechBattle.Views;

namespace ZE.MechBattle
{
    [Serializable]
    public class VirtualViewPartSetupData : VirtualViewSetupData
    {
        [field: SerializeField] public ViewPartKey PartKey { get; private set; }
    
    }
}
