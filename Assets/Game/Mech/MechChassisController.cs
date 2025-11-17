using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Movement
{
    public class MechChassisController : MonoBehaviour
    {
        [SerializeField] private LegView _leftLeg;
        [SerializeField] private LegView _rightLeg;
        [Space]
        [Range(0,1)][SerializeField] private float _stepDistanceCf = 0.5f;
        [Range(0, 0.99f)][SerializeField] private float _chassisDownCf = 0.9f;
        [SerializeField] private StepSettings _stepSettings;

        private bool _isProcessingStep = false;
        private bool _leftLegTurn = false;
        private float _hipsDistance;
        private float _steerValue;
        private float _speedValue;
        private Vector3 _defaultLeftFootPos;
        private Vector3 _defaultRightFootPos;
        private StepFrame _stepFrame;
        private ChassisParameters _chassisParameters;
        private float LegLength => _chassisParameters.LegLength;
        private float StepLength => _stepDistanceCf * LegLength;
        private float HipsLength => _chassisParameters.HipLength;
        private float AnkleLength => _chassisParameters.AnkleLength;
        private float HipsDistance => _chassisParameters.HipsDistance;

        // note: smooth, but can be too slow for small fast mechs
        private float ChassisRotationSpeed => _stepSettings.MaxSteerAngle / (_stepSettings.Duration * 1.5f);

        private void Start()
        {
            _defaultLeftFootPos = transform.InverseTransformPoint(_leftLeg.FootPosition);
            _defaultRightFootPos = transform.InverseTransformPoint(_rightLeg.FootPosition);

            var hipLength = Vector3.Distance(_leftLeg.HipPosition, _leftLeg.AnklePosition);
            var ankleLength = Vector3.Distance(_leftLeg.AnklePosition, _leftLeg.FootPosition);
            var hipsDistance = Vector3.Distance(_leftLeg.HipPosition, _rightLeg.HipPosition);
            _chassisParameters = new(hipLength: hipLength, ankleLength: ankleLength, hipsDistance: hipsDistance);

            _rightLeg.SetParameters(_chassisParameters);
            _leftLeg.SetParameters(_chassisParameters);
        }

        private void Update()
        {
            _speedValue = Input.GetAxis("Vertical");
            _steerValue = Input.GetAxis("Horizontal");

            if (_isProcessingStep)
            {
                var dt = Time.deltaTime;
                _stepFrame = _stepFrame.Update(dt);
                var currentPoint = _stepFrame.CurrentPoint;

                if (!_leftLegTurn)
                {
                    var leftFootPoint = _leftLeg.GetFootPoint();
                    PositionChassisCenter(leftFootPoint: leftFootPoint, rightFootPoint: currentPoint, dt);
                    _rightLeg.MoveLegToPoint(currentPoint);
                    _leftLeg.MoveLegToPoint(leftFootPoint);
                }
                else
                {                    
                    var rightFootPoint = _rightLeg.GetFootPoint();                   
                    PositionChassisCenter(leftFootPoint: currentPoint, rightFootPoint: rightFootPoint, dt);
                    _leftLeg.MoveLegToPoint(currentPoint);
                    _rightLeg.MoveLegToPoint(rightFootPoint);
                }
                if (_stepFrame.IsFinished)
                {
                    _leftLegTurn = !_leftLegTurn;
                    _isProcessingStep = false;
                }                
            }
            else
            {
                if (_speedValue != 0f || _steerValue != 0f)
                {
                    _isProcessingStep = true;
                    if (_leftLegTurn)
                    {
                        var prevTransform = _leftLeg.GetFootPoint();
                        _stepFrame = new StepFrame(prevTransform, DefineFootNextPosition(prevTransform.pos, _defaultLeftFootPos), _stepSettings);
                    }                        
                    else
                    {
                        var prevTransform = _rightLeg.GetFootPoint();
                        _stepFrame = new StepFrame(prevTransform, DefineFootNextPosition(prevTransform.pos, _defaultRightFootPos), _stepSettings);
                    }
                       
                }
            }
        }

        private RigidTransform DefineFootNextPosition(Vector3 footPosition, Vector3 defaultLocalPos)
        {
            var k = 1f;
            var moveDirection = math.forward();
            if (_steerValue != 0f)
            {
                var fwd = math.forward();
                var rotation = quaternion.AxisAngle(math.up(), _steerValue * _stepSettings.MaxSteerAngle * math.TORADIANS);
                moveDirection = math.mul(rotation, fwd);
                if (moveDirection.z < 0f) 
                    Debug.LogError($"{fwd} x {math.Euler(rotation)} = {moveDirection}");
                var sinAngle = math.length(math.cross(fwd, moveDirection));

                if (sinAngle != 0f)
                {
                    var x = StepLength / (math.sqrt(sinAngle)) - _hipsDistance;
                    k = x / (StepLength + x);

                    if (x < 0f)
                        Debug.LogError("negative x");
                }
                if (sinAngle < 0f)
                    Debug.LogError("negative sin");

               
            }
            Vector3 nextFootLocalPos;

            if (_speedValue != 0f)
            {
                float stepCf;
                if (moveDirection.x > 0 == !_leftLegTurn)
                    stepCf = k;
                else
                    stepCf = 1f;


                float3 localFootPos = transform.InverseTransformPoint(footPosition);
                nextFootLocalPos = localFootPos + _speedValue * stepCf * StepLength * moveDirection;

                // should not move leg more than half a step afore chassis center
                var xzMovementDir = new Vector3(nextFootLocalPos.x - defaultLocalPos.x, 0f, nextFootLocalPos.z - defaultLocalPos.z);
                var xzStepLength = Mathf.Clamp(xzMovementDir.magnitude, 0.1f, StepLength * 0.5f);
                nextFootLocalPos = xzStepLength * xzMovementDir.normalized + defaultLocalPos;               
            }
            else
            {
                // rotation without movement
                nextFootLocalPos = defaultLocalPos;
            }
            // todo: raycast check
            var nextPos = transform.TransformPoint(nextFootLocalPos);
            nextPos.y = 0f;

            return new(Quaternion.LookRotation(transform.TransformDirection(moveDirection), Vector3.up), nextPos);
        }

        private void PositionChassisCenter(RigidTransform leftFootPoint, RigidTransform rightFootPoint, float dt)
        {
            var dir = leftFootPoint.pos - rightFootPoint.pos;
            var halfDist = math.length(dir) * 0.5f;
            var height = math.sqrt(LegLength * LegLength - halfDist * halfDist) * _chassisDownCf;
            transform.position = rightFootPoint.pos + halfDist * math.normalize(dir)+ new float3(0f, height, 0f);

            var targetRotation = Quaternion.Lerp(rightFootPoint.rot, leftFootPoint.rot,  _steerValue * 0.5f + 0.5f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, ChassisRotationSpeed * dt);
        }
    }
}
