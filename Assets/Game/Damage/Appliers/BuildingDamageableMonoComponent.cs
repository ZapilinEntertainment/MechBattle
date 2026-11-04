using UnityEngine;

namespace ZE.MechBattle
{
    [AddComponentMenu(DirectoryConstants.BUILDING_COMPONENTS_PATH + "Health")]
    public class BuildingDamageableMonoComponent : MonoBehaviour
    {
        [field: SerializeField] public bool IsAttackTarget { get; private set; } 
        [field: SerializeField] public int HealthPoints { get; private set; }
        [field: SerializeField] public DestructionEffectProtocol DestructionProtocol { get; private set; }
    }
}
