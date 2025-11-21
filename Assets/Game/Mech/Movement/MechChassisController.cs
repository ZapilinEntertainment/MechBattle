using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Movement
{
    public class MechChassisController : MonoBehaviour
    {
        [SerializeField] private LegView _leftLegView;
        [SerializeField] private LegView _rightLegView;
        [Space]
        [Range(0, 0.99f)][SerializeField] private float _defaultChassisHeight = 0.93f;
        [Range(0, 0.99f)][SerializeField] private float _minStepChassisHeight = 0.9f;
        [Range(0.1f, 1f)][SerializeField] private float _stepLengthCf = 1f;
        [SerializeField] private StepSettings _stepSettings;

        private bool _isProcessingStep = false;
        private bool _leftLegTurn = false;
        private float _steerValue;
        private float _speedValue;
        private StepFrame _stepFrame;
        private Chassis _chassis;
        private IGroundCaster _groundCaster;
        private LegController _leftLeg;
        private LegController _rightLeg;
        private float LegLength => _chassis.LegLength;
        private float StepLength => _maxStepLength * _stepLengthCf;
        private float MaxHeightDelta => _chassis.AnkleLength;

        // note: smooth, but can be too slow for small fast mechs
        private float ChassisRotationSpeed => _stepSettings.MaxSteerAngle / (_stepSettings.Duration * 1.5f);
        private float _maxStepLength;

        private void Start()
        {
            var hipLength = Vector3.Distance(_leftLegView.Hip.position, _leftLegView.Ankle.position);
            var ankleLength = Vector3.Distance(_leftLegView.Ankle.position, _leftLegView.Foot.position);
            var hipsDistance = Vector3.Distance(_leftLegView.Hip.position, _rightLegView.Hip.position);
            _chassis = new(transform: transform, hipLength: hipLength, ankleLength: ankleLength, hipsDistance: hipsDistance);

            _leftLeg = new (_leftLegView,  _chassis);
            _rightLeg = new(_rightLegView,  _chassis);

            _groundCaster = new PhysicsGroundCaster();

            _maxStepLength = math.sin(_minStepChassisHeight * math.PI * 0.5f) * ankleLength;
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
            var defaultLocalPos = movingLeg.DefaultFootLocalPosition;
            var backLegLocalPos = _chassis.Transform.InverseTransformPoint(backLeg.CurrentFootPosition);
            backLegLocalPos.y = 0;
            var stepLength = StepLength;

            var moveDirection = Vector3.forward;
            var rotation = Quaternion.identity;
            if (_steerValue != 0f)
            {
                var fwd = math.forward();
                rotation = Quaternion.AngleAxis(_steerValue * _stepSettings.MaxSteerAngle, Vector3.up);
                moveDirection = math.mul(rotation, fwd);            
            }

            // next local pos should be outside the hips distance circle,
            // but inside max step circle
            // counting from other leg point

            var startPos = rotation * defaultLocalPos;
            startPos.y = 0f;

            var nextFootLocalPos = startPos + _speedValue * stepLength * moveDirection;
            var hipsDir = nextFootLocalPos - backLegLocalPos;
           // var cachedDir = moveDirection;
            var mindistance = _chassis.HipsDistance * 0.8f;
            if (hipsDir.sqrMagnitude < mindistance * mindistance)
            {
                // inside hip distance circle
                var intersection = backLegLocalPos + _chassis.HipsDistance * hipsDir.normalized;
                var iv = intersection - startPos;
                if (iv.sqrMagnitude > stepLength * stepLength)
                    iv = stepLength * iv.normalized;
                nextFootLocalPos = startPos + iv;
                moveDirection = iv.normalized;
               // Debug.Log($"backleg: {backLegLocalPos}, nextpos: {nextFootLocalPos}, inter: {intersection}, dist: {hipsDir.magnitude}");
               // Debug.Log($"too close, corrected: {cachedDir} -> {moveDirection}");
            }
            else
            {
                if (hipsDir.sqrMagnitude > _maxStepLength * _maxStepLength)
                {
                    // outside of max step circle
                    var intersection = backLegLocalPos + _maxStepLength * hipsDir.normalized;
                    var iv = intersection - startPos;
                    if (iv.sqrMagnitude > stepLength * stepLength)
                        iv = stepLength * iv.normalized;
                    nextFootLocalPos = startPos + iv;
                    moveDirection = iv.normalized;
                    //Debug.Log($"too far, corrected: {cachedDir} -> {moveDirection}");
                }
            }

            nextFootLocalPos.y = defaultLocalPos.y;
            moveDirection.y = 0;
            if (moveDirection.z < 0f)
                moveDirection *= -1f;

            var nextPosWorld = _chassis.Transform.TransformPoint(nextFootLocalPos);
            nextPosWorld.y = 0f;

            return AdjustNextStepAccordingToHeight(nextPosWorld, moveDirection, movingLeg);
        }

        private void PositionChassisCenter(RigidTransform leftFootPoint, RigidTransform rightFootPoint, float dt)
        {
            var dir = leftFootPoint.pos - rightFootPoint.pos;
            var halfDist = math.length(dir) * 0.5f;
            var height = math.sqrt(LegLength * LegLength - halfDist * halfDist) * _defaultChassisHeight;
            transform.position = rightFootPoint.pos + halfDist * math.normalize(dir)+ new float3(0f, height, 0f);

            var targetRotation = Quaternion.Lerp(rightFootPoint.rot, leftFootPoint.rot,  _steerValue * 0.5f + 0.5f);
            var targetForward = targetRotation * Vector3.forward;
            // cabin zero inclining
            targetRotation = Quaternion.LookRotation(targetForward, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, ChassisRotationSpeed * dt);
        }

        private RigidTransform AdjustNextStepAccordingToHeight(Vector3 targetFootPos, Vector3 moveVectorLocal, LegController leg)
        {
            // todo: do interface note if height is not reachable

            var currentLegPoint = leg.CurrentFootPoint;
            if (!_groundCaster.TryGetGroundPoint(targetFootPos.x, targetFootPos.z, out var point))
                return currentLegPoint;            

            var deltaHeight = leg.DefaultFootLocalPosition.y - _chassis.Transform.InverseTransformPoint(point.Position).y;

            if (math.abs(deltaHeight) > MaxHeightDelta)
            {
                var startFootPos = currentLegPoint.pos;
                var projectedStart = new float2(startFootPos.x,  startFootPos.z);
                var projectedEnd = new float2(targetFootPos.x, targetFootPos.z);
                var pos = math.lerp( projectedStart, projectedEnd, 0.5f);
                if (math.lengthsq(projectedStart - pos) < _chassis.HipsDistance * _chassis.HipsDistance * 0.25f)
                    return currentLegPoint;

                return AdjustNextStepAccordingToHeight(new Vector3(pos.x,0f, pos.y),moveVectorLocal, leg);
            }
            return new(Quaternion.LookRotation(transform.TransformDirection(moveVectorLocal), point.Normal), point.Position);
        }
    }
}
