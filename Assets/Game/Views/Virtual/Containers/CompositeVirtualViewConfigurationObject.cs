using UnityEngine;

namespace ZE.MechBattle.Views
{
    [CreateAssetMenu(fileName = nameof(CompositeVirtualViewConfigurationObject), menuName = "Scriptable Objects/" + nameof(CompositeVirtualViewConfigurationObject))]
    public class CompositeVirtualViewConfigurationObject : ScriptableObject
    {
        [field: SerializeField] public VirtualViewPartSetupData[] ViewPartsData { get; private set; }

    }
}
