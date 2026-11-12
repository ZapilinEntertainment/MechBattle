using UnityEngine;
using ZE.Utils;

namespace ZE.MechBattle
{
    public class MaterialsDepot : DynamicAssetDictionary<Material>
    {
        protected override void OnTemporalElementDisposed(Material element)
        {
            GameObject.Destroy(element);
        }

    }
}
