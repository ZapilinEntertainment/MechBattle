using UnityEngine;
using ZE.MechBattle.Vfx;

namespace ZE.MechBattle
{
    public class EnergyCellUiViewPool : MonoViewPoolBase<EnergyCellUiView>
    {
        public EnergyCellUiViewPool(EnergyCellUiView prefab, Transform hostOfPools) : base(prefab, hostOfPools)
        {
        }

        protected override string PoolHostObjectName => nameof(EnergyCellUiViewPool);

        protected override int DefaultCapacity => 16;
    }
}
