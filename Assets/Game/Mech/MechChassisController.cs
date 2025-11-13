using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    public class MechChassisController : MonoBehaviour
    {
        [SerializeField] private Transform _leftHip;
        [SerializeField] private Transform _leftAnkle;
        [SerializeField] private Transform _leftFoot;
        [Space]
        [SerializeField] private Transform _rightHip;
        [SerializeField] private Transform _rightAnkle;
        [SerializeField] private Transform _rightFoot;
        [Space]
        [Range(0,1)][SerializeField] private float _stepDistanceCf = 0.5f;
        [Range(0, 0.99f)][SerializeField] private float _chassisDownCf = 0.9f;

        private bool _isProcessingStep = false;
        private bool _leftLegTurn = false;
        private float _hipLength;
        private float _ankleLength;
        private Vector3 _defaultLeftFootPos;
        private Vector3 _defaultRightFootPos;
        private float LegLength => _ankleLength + _hipLength;
        private float StepLength => _stepDistanceCf * LegLength;

        private void Start()
        {
            _defaultLeftFootPos = transform.InverseTransformPoint(_leftFoot.position);
            _defaultRightFootPos = transform.InverseTransformPoint(_rightFoot.position);

            _hipLength = Vector3.Distance(_leftHip.position, _leftAnkle.position);
            _ankleLength = Vector3.Distance(_leftAnkle.position, _leftFoot.position);
        }

        private void Update()
        {
            if (_isProcessingStep)
            {
                if (_leftLegTurn)
                {
                    var nextPos = DefineRightLegNextPosition();
                    var leftFootPos = _leftFoot.position;
                    PositionChassisCenter(leftLegPos: leftFootPos, rightLegPos: nextPos);
                    MoveLegToPosition(nextPos, _rightHip, _rightAnkle, _rightFoot);
                    MoveLegToPosition(leftFootPos, _leftHip, _leftAnkle, _leftFoot);
                    _leftFoot.position = leftFootPos;
                }
                else
                {
                    var nextPos = DefineLeftLegNextPosition();
                    var rightFootPos = _rightFoot.position;
                    PositionChassisCenter(leftLegPos: nextPos, rightLegPos: rightFootPos);
                    MoveLegToPosition(nextPos, _leftHip, _leftAnkle, _leftFoot);
                    MoveLegToPosition(rightFootPos, _rightHip, _rightAnkle, _rightFoot);
                    _rightFoot.position = rightFootPos;
                }
                _leftLegTurn = !_leftLegTurn;
                _isProcessingStep = false;
            }
            else
            {
                _isProcessingStep = Input.GetKeyDown(KeyCode.Space);
            }
        }

        private Vector3 DefineLeftLegNextPosition()
        {
            var pos = transform.InverseTransformPoint(_leftFoot.position).z + StepLength;
            pos = math.min(pos, StepLength * 0.5f);

            // todo: steer shift
            // todo: raycast check
            var result = transform.TransformPoint(_defaultLeftFootPos.x, _defaultLeftFootPos.y, pos);
            result.y = 0;
            return result;
        }

        private Vector3 DefineRightLegNextPosition()
        {
            var pos = transform.InverseTransformPoint(_rightFoot.position).z + StepLength;
            pos = math.min(pos, StepLength * 0.5f);
            // todo: steer shift
            // todo: raycast check
            var result = transform.TransformPoint(_defaultRightFootPos.x, _defaultRightFootPos.y, pos);
            result.y = 0;
            return result;
        }

        private void PositionChassisCenter(Vector3 leftLegPos, Vector3 rightLegPos)
        {
            var dir = leftLegPos - rightLegPos;
            var halfDist = dir.magnitude * 0.5f;
            var height = math.sqrt(LegLength * LegLength - halfDist * halfDist) * _chassisDownCf;
            transform.position = rightLegPos + halfDist * dir.normalized + height * Vector3.up;
        }

        private void MoveLegToPosition(Vector3 pos, Transform hip, Transform ankle, Transform foot)
        {
            var hipPosition = hip.position;
            var dir = pos - hipPosition;
            var directLength = dir.magnitude;
            var a = _hipLength * _hipLength + directLength * directLength - _ankleLength * _ankleLength;
            var b = 2 * _hipLength * directLength;
            var cosA = a / b;

            var x = cosA * _hipLength;
            var y = math.sqrt(_hipLength * _hipLength - x * x);
            var right = Vector3.ProjectOnPlane(transform.right, Vector3.up);
            var upVector = Vector3.Cross(dir, right);
            var middlePoint = hipPosition + x * dir.normalized + y * upVector.normalized;

            var hipDir = (middlePoint - hipPosition).normalized;
            hip.rotation = Quaternion.LookRotation(hipDir, Vector3.Cross(hipDir, hip.right).normalized);

            var ankleDir = (pos - middlePoint).normalized;
            ankle.position = middlePoint;
            ankle.rotation = Quaternion.LookRotation(ankleDir, Vector3.Cross(ankleDir, ankle.right).normalized);

            foot.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            foot.position = pos;
        }
    }
}
