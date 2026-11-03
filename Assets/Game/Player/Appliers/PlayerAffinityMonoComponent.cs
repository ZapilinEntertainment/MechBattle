using UnityEngine;

namespace ZE.MechBattle
{
    [AddComponentMenu(DirectoryConstants.BUILDING_COMPONENTS_PATH + "Affinity")]
    public class PlayerAffinityMonoComponent : MonoBehaviour
    {
        [field: SerializeField] public PlayerKey PlayerKey;
    }
}
