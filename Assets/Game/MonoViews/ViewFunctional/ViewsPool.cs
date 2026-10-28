using UnityEngine;
using ZE.Utils;

namespace ZE.MechBattle.Views
{
    public class ViewsPool : MonoViewPoolBase<PoolableMonoView>, IViewProvider
    {

        public ViewsPool(string assetName, PoolableMonoView prefab, Transform hostOfPools) : base(
            prefab, 
            hostOfPools,
            assetName + "_pool",
            defaultCapacity : 8,
            maxCapacity: GameConstants.MAX_VIEWS_COUNT)
        {
        }

        public bool IsReadyToProvide => true;

        // TODO: need to specific assets capacity

        public IView GetView() => Get();
    }
}
