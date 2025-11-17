using UnityEngine;
using Unity.Mathematics;

namespace ZE.MechBattle.Movement
{
    public readonly struct StepFrame
    {
        public bool IsFinished => Progress == 1f;
        public RigidTransform CurrentPoint
        {
            get
            {
                var dir = Vector3.Lerp(StartPoint.pos, _targetPosXZ, Settings.SpeedCurve.Evaluate(Progress));
                var riseHeight = Settings.StepRaiseHeight * Settings.HeightCurve.Evaluate(Progress);
                var height = Mathf.Lerp(StartPoint.pos.y, TargetPoint.pos.y, Progress) + riseHeight;
                dir.y = Mathf.Clamp(height, _minHeight, _maxHeight + Settings.StepRaiseHeight);

                var rot = Quaternion.Slerp(StartPoint.rot, TargetPoint.rot, Progress);
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
            var planeProjection = dir.ProjectOnPlane(Vector3.up);
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
            var progress = Mathf.MoveTowards(Progress, 1f, deltaTime / Settings.Duration);
            return new StepFrame(this, progress);
        }
    }
}
