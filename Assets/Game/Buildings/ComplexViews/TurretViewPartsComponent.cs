using UnityEngine;

namespace ZE.MechBattle
{
    [AddComponentMenu(DirectoryConstants.BUILDING_VIEWPARTS_PATH + "TurretParts")]
    public class TurretViewPartsComponent : BuildingViewPartsComponent
    {
        [SerializeField] private Transform _tower;
        [SerializeField] private Transform _barrel;

        public override bool TryGetViewPart(ViewPartKey key, out IViewPart viewPart)
        {
            switch (key.Type)
            {
                case ViewPartType.Tower:
                    {
                        viewPart = new ViewPartContainer(_tower); 
                        return true;
                    }
                case ViewPartType.Barrel:
                    {
                        viewPart = new ViewPartContainer(_barrel);
                        return true;
                    }
                default:
                    {
                        viewPart = default;
                        return false;
                    }
            }
        }
    }
}
