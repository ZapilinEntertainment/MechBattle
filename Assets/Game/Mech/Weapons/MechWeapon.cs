using System;
using UnityEngine;
using R3;

namespace ZE.MechBattle.Weapons
{
    public abstract class MechWeapon : MonoBehaviour
    {
        [SerializeField] protected Transform _aimingPart;
        public abstract float YRotationLimitDegrees { get; }
        public abstract float XRotationLimitDegrees { get; }
        public abstract float AimSpeed { get; }
        public abstract bool ShowInterfaceAim { get; }
        public Transform AimingPart => _aimingPart;
        public ITargetDesignator TargetDesignator { get; private set; }
        private TargetData _targetData;
        private IDisposable _designatorSubscription;
        private float _yRotationDotLimit;
        private float _xRotationDotLimit;


        public virtual void SetDesignator(ITargetDesignator designator)
        {
            _designatorSubscription?.Dispose();

            TargetDesignator = designator;
            _designatorSubscription = TargetDesignator
                .TargetDataProperty
                .Subscribe(data => _targetData = data);
        }

        public abstract void Fire();

        private void Start()
        {
            _xRotationDotLimit = Mathf.Cos(XRotationLimitDegrees * Mathf.Deg2Rad);
            _yRotationDotLimit = Mathf.Cos(YRotationLimitDegrees * Mathf.Deg2Rad);
        }

        private void Update()
        {
            if (!_targetData.IsDefined)
                return;
            var dir = LimitGunAimVector(_targetData.Position - _aimingPart.position);
           var targetRotation = Quaternion.LookRotation(dir.normalized, transform.up);
            _aimingPart.rotation = Quaternion.RotateTowards(_aimingPart.rotation, targetRotation, AimSpeed * Time.deltaTime);
        }

        private Vector3 LimitGunAimVector(Vector3 targetDirection)
        {
             var localDir = transform.InverseTransformDirection(targetDirection).normalized;
            var fwd = Vector3.forward;
            var azimuth = Vector3.Dot( new Vector3(localDir.x, 0f, localDir.z).normalized, fwd);

            if (azimuth < _yRotationDotLimit)
            {
                var height = localDir.y;
                localDir = Quaternion.AngleAxis(YRotationLimitDegrees, localDir.x > 0 ? Vector3.up : Vector3.down) * fwd;
                localDir.y = height;                
            }

            var elevation = Vector3.Dot(new Vector3(0f, localDir.y, localDir.z).normalized, fwd);
            if (elevation < _xRotationDotLimit)
            {
                var xpos = localDir.x;
                localDir = Quaternion.AngleAxis(XRotationLimitDegrees, localDir.y > 0f ? Vector3.left : Vector3.right) * fwd;
                localDir.x = xpos;
            }
            return transform.TransformDirection(localDir);
        }

        private void OnDestroy()
        {
            _designatorSubscription?.Dispose();
        }
    }
}
