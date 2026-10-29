using UnityEngine;
namespace ZE.MechBattle
{
    public abstract class BuildingViewPartsComponent : MonoBehaviour
    {
        public abstract bool TryGetViewPart(ViewPartKey key, out IViewPart viewPart);
    }
}
