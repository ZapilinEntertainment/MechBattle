using UnityEngine;
using ZE.Utils;

namespace ZE.MechBattle
{
    public class EnergyCellUiViewPool : MonoViewPoolBase<EnergyCellUiView>
    {
        public EnergyCellUiViewPool(EnergyCellUiView prefab, Transform hostOfPools) : base(prefab, hostOfPools, nameof(EnergyCellUiViewPool), 16, 128)
        {
        }
    }
}
