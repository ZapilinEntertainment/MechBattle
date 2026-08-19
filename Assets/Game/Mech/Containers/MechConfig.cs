using Unity.Mathematics;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechConfig), menuName = "Scriptable Objects/" + nameof(MechConfig))]
    public class MechConfig : ScriptableObject
    {
        [Header("Head")]
        [field: SerializeField] public ViewPartAttachmentProtocol HeadAttachmentProtocol { get; private set; }
        [field:SerializeField] public ForwardRotationLimits HeadRotationLimits { get; private set; }
        [field: SerializeField] public float3 LeftEyeLocalPosition { get; private set; }
        [field: SerializeField] public float3 RightEyeLocalPosition { get; private set; }
        [SerializeField] private float _headRotationSpeedDegrees;
        public float HeadRotationSpeedRadians => math.radians(_headRotationSpeedDegrees);


        [Space]
        [SerializeField] private float _upperPartRotationSpeedDegrees = 90f;       
        [SerializeField] private SerializedDictionary<MechSlot, MechSlotInfo> _slotInfo;
       

        public float UpperPartRotationSpeedRadians => math.radians(_upperPartRotationSpeedDegrees);

        public bool TryGetSlotInfo(MechSlot slot, out MechSlotInfo slotInfo) => _slotInfo.TryGetValue(slot, out slotInfo);
    }
}
