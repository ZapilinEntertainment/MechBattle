using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class LegView : MonoBehaviour
    {
        [SerializeField] private Transform _hip;
        [SerializeField] private Transform _ankle;
        [SerializeField] private Transform _foot;

        private ChassisParameters _parameters;
        private float HipLength => _parameters.HipLength;
        private float AnkleLength => _parameters.AnkleLength;

        public Vector3 FootPosition => _foot.position;
        public Vector3 AnklePosition => _ankle.position;
        public Vector3 HipPosition => _hip.position;
        public RigidTransform GetFootPoint() => _foot.ToRigidTransform();

        public void SetParameters(ChassisParameters parameters) => _parameters = parameters;

        public void MoveLegToPoint(RigidTransform point)
        {
            var hipPosition = (float3)_hip.position;
            var pos = point.pos;
            var dir = pos - hipPosition;
            var directLength = math.length(dir);
            var a = HipLength * HipLength + directLength * directLength - AnkleLength * AnkleLength;
            var b = 2 * HipLength * directLength;
            var cosA = a / b;

            var x = cosA * HipLength;
            var y = math.sqrt(math.abs(HipLength * HipLength - x * x));

            var right = transform.right;
            right.y = 0;
            right = right.normalized;

            var upVector = math.cross(dir, right);
            var middlePoint = hipPosition + x * math.normalize(dir) + y * math.normalize(upVector);

            var hipDir = math.normalize(middlePoint - hipPosition);

            // todo: Need investigation and fix!
            if (math.lengthsq(hipDir) == math.EPSILON)
                return;

            upVector = math.normalize(math.cross(hipDir, point.GetRightVector()));
            _hip.rotation = Quaternion.LookRotation(hipDir, upVector);

            var ankleDir = math.normalize(pos - middlePoint);
            _ankle.position = middlePoint;
            _ankle.rotation = Quaternion.LookRotation(ankleDir, Vector3.Cross(ankleDir, _ankle.right).normalized);

            _foot.rotation = point.rot;
            _foot.position = pos;
        }
    }
}
