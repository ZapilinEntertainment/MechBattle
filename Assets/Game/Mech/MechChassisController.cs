using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Movement
{
    public class MechChassisController : MonoBehaviour
    {
        [SerializeField] private LegView _leftLegView;
        [SerializeField] private LegView _rightLegView;
        [Space]
        [SerializeField] private float _stepDistanceCf = 0.5f;
        [Range(0, 0.99f)][SerializeField] private float _chassisDownCf = 0.9f;
        [SerializeField] private StepSettings _stepSettings;

        private bool _isProcessingStep = false;
        private bool _leftLegTurn = false;
        private float _hipsDistance;
        private float _steerValue;
        private float _speedValue;
        private StepFrame _stepFrame;
        private Chassis _chassis;
        private IGroundCaster _groundCaster;
        private LegController _leftLeg;
        private LegController _rightLeg;
        private float LegLength => _chassis.LegLength;
        private float MaxStepLength => _stepDistanceCf * LegLength;

        // note: smooth, but can be too slow for small fast mechs
        private float ChassisRotationSpeed => _stepSettings.MaxSteerAngle / (_stepSettings.Duration * 1.5f);

        private void Start()
        {
            var hipLength = Vector3.Distance(_leftLegView.Hip.position, _leftLegView.Ankle.position);
            var ankleLength = Vector3.Distance(_leftLegView.Ankle.position, _leftLegView.Foot.position);
            var hipsDistance = Vector3.Distance(_leftLegView.Hip.position, _rightLegView.Hip.position);
            _chassis = new(transform: transform, hipLength: hipLength, ankleLength: ankleLength, hipsDistance: hipsDistance);

            _leftLeg = new (_leftLegView,  _chassis);
            _rightLeg = new(_rightLegView,  _chassis);

            _groundCaster = new PhysicsGroundCaster();
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
                    var leftFootPoint = _leftLegView.GetFootPoint();
                    PositionChassisCenter(leftFootPoint: leftFootPoint, rightFootPoint: currentPoint, dt);
                    _rightLeg.MoveLegToPoint(currentPoint);
                    _leftLeg.MoveLegToPoint(leftFootPoint);
                }
                else
                {                    
                    var rightFootPoint = _rightLegView.GetFootPoint();                   
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
                    RigidTransform prevPoint;
                    RigidTransform nextPoint;
                    if (_leftLegTurn)
                    {
                        prevPoint = _leftLegView.GetFootPoint();
                        nextPoint = DefineFootNextPosition(_leftLeg, _rightLeg);                        
                    }                        
                    else
                    {
                        prevPoint = _rightLegView.GetFootPoint();
                        nextPoint = DefineFootNextPosition(_rightLeg, _leftLeg);
                    }

                    _stepFrame = new StepFrame(prevPoint, nextPoint, _stepSettings);
                    if (math.lengthsq(prevPoint.pos - nextPoint.pos) < math.EPSILON * math.EPSILON 
                        && math.angle(prevPoint.rot, nextPoint.rot) < math.EPSILON)
                    {
                        _leftLegTurn = !_leftLegTurn;
                        _isProcessingStep = false;                        
                    }                                              
                }
            }
        }

        private RigidTransform DefineFootNextPosition(LegController movingLeg, LegController backLeg)
        {
            var footPoint = movingLeg.CurrentFootPoint;
            var footPosition = footPoint.pos;
            var defaultLocalPos = movingLeg.DefaultFootLocalPosition;

            var k = 1f;
            var moveDirection = math.forward();
            if (_steerValue != 0f)
            {
                var fwd = math.forward();
                var rotation = quaternion.AxisAngle(math.up(), _steerValue * _stepSettings.MaxSteerAngle * math.TORADIANS);
                moveDirection = math.mul(rotation, fwd);
               // if (moveDirection.z < 0f) 
               //     Debug.LogError($"{fwd} x {math.Euler(rotation)} = {moveDirection}");
                var sinAngle = math.length(math.cross(fwd, moveDirection));

                if (sinAngle != 0f)
                {
                    var x = MaxStepLength / (math.sqrt(sinAngle)) - _hipsDistance;
                    k = x / (MaxStepLength + x);
                }               
            }
            float3 nextFootLocalPos;

            if (_speedValue != 0f)
            {
                float stepCf;
                if (moveDirection.x > 0 == !_leftLegTurn)
                    stepCf = k;
                else
                    stepCf = 1f;

                //Debug.Log($"{(_leftLegTurn ? "step left" : "step right")} : {stepCf}");


                float3 localFootPos = transform.InverseTransformPoint(footPosition);
                nextFootLocalPos = localFootPos + _speedValue * stepCf * MaxStepLength * moveDirection;
                nextFootLocalPos.z = math.clamp(nextFootLocalPos.z, -MaxStepLength * 0.5f, MaxStepLength * 0.5f);  

                // should not move leg more than half a step afore chassis center
                var xzMovementDir = new Vector3(nextFootLocalPos.x - defaultLocalPos.x, 0f, nextFootLocalPos.z - defaultLocalPos.z);
                nextFootLocalPos = xzMovementDir + defaultLocalPos;
                if (_leftLegTurn)
                    nextFootLocalPos.x = math.clamp(nextFootLocalPos.x, -MaxStepLength * 0.5f, _leftLeg.DefaultFootLocalPosition.x) ;
                else
                    nextFootLocalPos.x = math.clamp(nextFootLocalPos.x, _rightLeg.DefaultFootLocalPosition.x, MaxStepLength * 0.5f);
            }
            else
            {
                // rotation without movement
                nextFootLocalPos = defaultLocalPos;
            }

            // check if next step is too high
            // already found more closer option if possible
            var nextPos = _chassis.Transform.TransformPoint(nextFootLocalPos);
            nextPos = AdjustNextStepAccordingToOtherLeg(movingLeg, nextPos, backLeg.CurrentFootPosition);
            return AdjustNextStepAccordingToHeight(footPoint, nextPos, moveDirection, movingLeg);
        }

        private void PositionChassisCenter(RigidTransform leftFootPoint, RigidTransform rightFootPoint, float dt)
        {
            var dir = leftFootPoint.pos - rightFootPoint.pos;
            var halfDist = math.length(dir) * 0.5f;
            var height = math.sqrt(LegLength * LegLength - halfDist * halfDist) * _chassisDownCf;
            transform.position = rightFootPoint.pos + halfDist * math.normalize(dir)+ new float3(0f, height, 0f);

            var targetRotation = Quaternion.Lerp(rightFootPoint.rot, leftFootPoint.rot,  _steerValue * 0.5f + 0.5f);
            var targetForward = targetRotation * Vector3.forward;
            // cabin zero inclining
            targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, ChassisRotationSpeed * dt);
        }

        // prevents foot overlapping
        private Vector3 AdjustNextStepAccordingToOtherLeg(
            LegController movingLeg,
            Vector3 targetFootPos, 
            Vector3 otherFootCurrentPos)
        {
           
            var startFootPos = movingLeg.CurrentFootPosition;
            startFootPos.y = 0f;
            targetFootPos.y = 0f;
            otherFootCurrentPos.y = 0f;

            var dir = targetFootPos - startFootPos;
            var footRadius = _stepSettings.FootRadius;            

            var dirToOtherFootCurrentPos = otherFootCurrentPos - targetFootPos;
            if (dirToOtherFootCurrentPos.sqrMagnitude < footRadius * footRadius)
            {
                return otherFootCurrentPos + _stepSettings.FootRadius * 2f * Vector3.Cross(dir.normalized, _leftLegTurn ? Vector3.up : Vector3.down);
            }
            return targetFootPos;
        }

        // note: use cached start foot point, not read from leg, because it already can be changed
        private RigidTransform AdjustNextStepAccordingToHeight(RigidTransform startFootPoint, Vector3 targetFootPos, Vector3 moveVectorLocal, LegController leg)
        {
            if (!_groundCaster.TryGetGroundPoint(targetFootPos.x, targetFootPos.z, out var point))
                return startFootPoint;            

            var hip = leg.HipPosition;
            var dir = point.Position - hip;

            var maxDistance = _chassis.LegLength + _chassis.HipsDistance + MaxStepLength;
            if (dir.sqrMagnitude > maxDistance * maxDistance)
            {
                var startFootPos = startFootPoint.pos;
                var projectedStart = new float2(startFootPos.x,  startFootPos.z);
                var projectedEnd = new float2(targetFootPos.x, targetFootPos.z);
                var pos = math.lerp( projectedStart, projectedEnd, 0.5f);
                if (math.lengthsq(projectedStart - pos) < _stepSettings.FootRadius * _stepSettings.FootRadius * 0.25f)
                    return startFootPoint;

                Debug.Log("cut " + Time.frameCount);
                return AdjustNextStepAccordingToHeight(startFootPoint, new Vector3(pos.x,0f, pos.y),moveVectorLocal, leg);
            }
            return new(Quaternion.LookRotation(transform.TransformDirection(moveVectorLocal), point.Normal), point.Position);
        }
    }
}
