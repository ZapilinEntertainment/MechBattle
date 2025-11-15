using UnityEngine;

namespace ZE.MechBattle.Movement
{
    public readonly struct StepFrame
    {
        public bool IsFinished => Progress == 1f;
        public Vector3 Position
        {
            get
            {
                var dir = Vector3.Lerp(StartPos, _targetPosXZ, Settings.SpeedCurve.Evaluate(Progress));
                var riseHeight = Settings.StepRaiseHeight * Settings.HeightCurve.Evaluate(Progress);
                var height = Mathf.Lerp(StartPos.y, TargetPos.y, Progress) + riseHeight;
                dir.y = Mathf.Clamp(height, _minHeight, _maxHeight + Settings.StepRaiseHeight);
                return dir;
            }
        }

        public readonly float Progress;
        public readonly Vector3 StartPos;
        public readonly Vector3 TargetPos;
        public readonly StepSettings Settings;

        private readonly Vector3 _targetPosXZ;
        private readonly float _minHeight;
        private readonly float _maxHeight;
        
        public StepFrame(Vector3 start, Vector3 end, StepSettings settings)
        {
            StartPos = start;
            TargetPos = end;
            Settings = settings;
            Progress = 0f;

            var dir = TargetPos - StartPos;
            var planeProjection = Vector3.ProjectOnPlane(dir, Vector3.up);
            _targetPosXZ = StartPos + planeProjection;

            if (StartPos.y > TargetPos.y)
            {
                _minHeight = TargetPos.y;
                _maxHeight = StartPos.y;
            }
            else
            {
                _maxHeight = TargetPos.y;
                _minHeight = StartPos.y;
            }
        }

        private StepFrame(StepFrame previous, float progress) 
        {
            StartPos = previous.StartPos;
            TargetPos = previous.TargetPos;
            Settings = previous.Settings; 

            Progress = progress;
            _targetPosXZ = previous._targetPosXZ;
            _minHeight = previous._minHeight;
            _maxHeight = previous._maxHeight;
        }

        public StepFrame Update(float deltaTime)
        {
            var progress = Mathf.MoveTowards(Progress, 1f, deltaTime / Settings.Duration);
            return new StepFrame(this, progress);
        }
    }
}
