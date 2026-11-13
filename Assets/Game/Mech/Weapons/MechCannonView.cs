using UnityEngine;

namespace ZE.MechBattle
{
    public class MechCannonView : ComplexMonoView
    {
        [SerializeField] private Transform _aimingPart;

        public override bool TryGetMonoPartView(ViewPartKey key, out IMonoView viewPart)
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
