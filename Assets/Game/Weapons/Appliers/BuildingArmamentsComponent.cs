using UnityEngine;

namespace ZE.MechBattle
{
    [AddComponentMenu(DirectoryConstants.BUILDING_COMPONENTS_PATH + "Armaments")]
    public class BuildingArmamentsComponent : MonoBehaviour
    {
        [field: SerializeField] public float TargetSearchRadius { get; private set; }
        [field: SerializeField] public EntityArmamentConfig[] Armaments { get; private set; }
    

    }
}
