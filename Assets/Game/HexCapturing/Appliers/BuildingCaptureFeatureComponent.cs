using UnityEngine;

namespace ZE.MechBattle
{
    [AddComponentMenu(DirectoryConstants.BUILDING_COMPONENTS_PATH + "Capturing")]
    public class BuildingCaptureFeatureComponent : MonoBehaviour
    {
        [field:SerializeField] public CaptureBehaviour CaptureBehaviour { get; private set; }
    
    }
}
