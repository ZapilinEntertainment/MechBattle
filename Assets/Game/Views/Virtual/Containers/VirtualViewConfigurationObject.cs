using UnityEngine;

namespace ZE.MechBattle.Views
{
    [CreateAssetMenu(fileName = nameof(VirtualViewConfigurationObject), menuName = "Scriptable Objects/" + nameof(VirtualViewConfigurationObject))]
    public class VirtualViewConfigurationObject : ScriptableObject
    {
        [field: SerializeField] public VirtualViewSetupData Data { get; private set; }
    }
}
