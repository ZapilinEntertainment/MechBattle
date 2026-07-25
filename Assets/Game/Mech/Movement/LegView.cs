using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class LegView : MonoBehaviour
    {
        [SerializeField] private Transform _hip;
        [SerializeField] private Transform _ankle;
        [SerializeField] private Transform _foot;

        public RigidTransform GetFootPoint() => _foot.ToRigidTransform();
        public Transform Hip => _hip;
        public Transform Ankle => _ankle;
        public Transform Foot => _foot;

        public LegDataContainer<Transform> ToContainer() => new() { Hip = _hip, Ankle = _ankle, Foot = _foot };
    }
}
