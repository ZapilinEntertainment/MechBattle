using UnityEngine;
using ZE.UiService;

namespace ZE.MechBattle.UI
{
    public class UIAimWindow : UiWindow
    {
        [field: SerializeField] public UIWeaponAimTracker AimTrackerPrefab { get; private set; }
        [field: SerializeField] public Transform MarkersHost;
    
    }
}
