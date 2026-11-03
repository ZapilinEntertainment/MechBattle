using UnityEngine;

namespace ZE.MechBattle
{
    [AddComponentMenu(DirectoryConstants.BUILDING_COMPONENTS_PATH + "Armaments")]
    public class BuildingArmamentsComponent : MonoBehaviour
    {
        [field: SerializeField] public float TargetSearchRadius { get; private set; }
        [field: SerializeField] public float MinTargetDistance { get; private set; }
        [field: SerializeField] public float TargetSearchCd { get; private set; } = 10f;
        [field: SerializeField] public EntityArmamentConfig[] Armaments { get; private set; }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, TargetSearchRadius);
        }
#endif
    }
}
