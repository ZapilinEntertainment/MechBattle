using UnityEngine;

namespace ZE.MechBattle
{
    public class MechCannonView : SimpleView, IComplexMonoView
    {
        [SerializeField] private Transform _aimingPart;

        public bool TryGetPartByKey(ViewPartKey key, out IViewPart viewPart)
        {
            if (key.Type == ViewPartType.Barrel)
            {
                viewPart = new ViewPartContainer(_aimingPart);
                return true;
            }

            viewPart = default;
            return false;
        }
    }
}
