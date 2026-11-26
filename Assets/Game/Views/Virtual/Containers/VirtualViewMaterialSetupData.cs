using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [Serializable]
    public class VirtualViewMaterialSetupData
    {
        [field: SerializeField] public string MaterialKey { get; private set; }
        [field: SerializeField] public bool ApplyFactionColor { get; private set; }

    }
}
