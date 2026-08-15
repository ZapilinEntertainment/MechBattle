using Unity.Mathematics;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechConfig), menuName = "Scriptable Objects/" + nameof(MechConfig))]
    public class MechConfig : ScriptableObject
    {
        [SerializeField] private float _upperPartRotationSpeedDegrees = 90f;
        [SerializeField] private SerializedDictionary<MechSlot, MechSlotInfo> _slotInfo;
       

        public float UpperPartRotationSpeedRadians => math.radians(_upperPartRotationSpeedDegrees);

        public bool TryGetSlotInfo(MechSlot slot, out MechSlotInfo slotInfo) => _slotInfo.TryGetValue(slot, out slotInfo);
    }
}
