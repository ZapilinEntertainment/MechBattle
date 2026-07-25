using Unity.Mathematics;

namespace ZE.MechBattle.MechMovement
{
    public readonly struct StepFrame
    {
        public bool IsFinished => Progress == 1f;
        public RigidTransform CurrentPoint
        {
            get
            {
                var dir = math.lerp(StartPoint.pos, _targetPosXZ, Settings.EvaluateSpeedCf(Progress));
                var riseHeight = Settings.StepRaiseHeight * Settings.EvaluateHeightCf(Progress);
                var height = math.lerp(StartPoint.pos.y, TargetPoint.pos.y, Progress) + riseHeight;
                dir.y = math.clamp(height, _minHeight, _maxHeight + Settings.StepRaiseHeight);

                var rot = math.slerp(StartPoint.rot, TargetPoint.rot, Progress);
                return new(rot, dir);
            }
        }

        public readonly float Progress;
        public readonly RigidTransform StartPoint;
        public readonly RigidTransform TargetPoint;
        public readonly StepSettings Settings;

        private readonly float3 _targetPosXZ;
        private readonly float _minHeight;
        private readonly float _maxHeight;
        
        public StepFrame(RigidTransform start, RigidTransform end, StepSettings settings)
        {
            StartPoint = start;
            TargetPoint = end;
            Settings = settings;
            Progress = 0f;

            var startPos = StartPoint.pos;
            var targetPos = TargetPoint.pos;
            var dir = targetPos - startPos;
            var planeProjection = dir.ProjectOnPlane(math.up());
            _targetPosXZ = startPos + planeProjection;

            if (startPos.y > targetPos.y)
            {
                _minHeight = targetPos.y;
                _maxHeight = startPos.y;
            }
            else
            {
                _maxHeight = targetPos.y;
                _minHeight = startPos.y;
            }
        }

        private StepFrame(StepFrame previous, float progress) 
        {
            StartPoint = previous.StartPoint;
            TargetPoint = previous.TargetPoint;
            Settings = previous.Settings; 

            Progress = progress;
            _targetPosXZ = previous._targetPosXZ;
            _minHeight = previous._minHeight;
            _maxHeight = previous._maxHeight;
        }

        public StepFrame Update(float deltaTime)
        {
            var progress = MathExtensions.MoveTowards(Progress, 1f, deltaTime / Settings.Duration);
            return new StepFrame(this, progress);
        }
    }
}
