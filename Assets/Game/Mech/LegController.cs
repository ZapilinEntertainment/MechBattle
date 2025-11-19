using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Movement
{
    public class LegController
    {
        public readonly Vector3 DefaultFootLocalPosition;
        public Vector3 CurrentFootPosition => _view.Foot.position;
        public RigidTransform CurrentFootPoint => _view.GetFootPoint();
        public Vector3 HipPosition => _view.Hip.position;
       

        private readonly LegView _view;
        private readonly Chassis _chassis;        

        private float HipLength => _chassis.HipLength;
        private float AnkleLength => _chassis.AnkleLength;
    
        public LegController(LegView view, Chassis chassis)
        {
            _view = view;
            _chassis = chassis;

            DefaultFootLocalPosition = chassis.Transform.InverseTransformPoint(view.Foot.position);
        }


        public void MoveLegToPoint(RigidTransform point)
        {
            var hipPosition = (float3)_view.Hip.position;
            var pos = point.pos;
            var dir = pos - hipPosition;
            var directLength = math.length(dir);
            var a = HipLength * HipLength + directLength * directLength - AnkleLength * AnkleLength;
            var b = 2 * HipLength * directLength;
            var cosA = a / b;

            var x = cosA * HipLength;
            var y = math.sqrt(math.abs(HipLength * HipLength - x * x));

            var right = point.GetRightVector();
            var upVector = math.cross(dir, right);
            var middlePoint = hipPosition + x * math.normalize(dir) + y * math.normalize(upVector);

            var hipDir = math.normalize(middlePoint - hipPosition);

            // todo: Need investigation and fix!
            if (math.lengthsq(hipDir) == math.EPSILON)
                return;

            upVector = math.normalize(math.cross(hipDir, right));
            _view.Hip.rotation = Quaternion.LookRotation(hipDir, upVector);

            var ankleDir = math.normalize(pos - middlePoint);
            _view.Ankle.position = middlePoint;
            upVector = math.cross(ankleDir, right);
            _view.Ankle.rotation = Quaternion.LookRotation(ankleDir, upVector);

            _view.Foot.rotation = point.rot;
            _view.Foot.position = pos;
        }
    }
}
