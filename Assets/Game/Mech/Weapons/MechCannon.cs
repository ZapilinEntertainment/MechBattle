using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Weapons
{
    public class MechCannon : MechWeapon
    {
        [SerializeField] private float _aimSpeed = 30f;
        [SerializeField] private Vector2 _aimLimits;
        [SerializeField] private Transform _gunPoint;
        [SerializeField] private string _projectileId;
        [Inject] private ProjectileRequestsFactory _requestsFactory;

        public override bool ShowInterfaceAim => true;
        public override float AimSpeed => _aimSpeed;
        public override float YRotationLimitDegrees => _aimLimits.y;
        public override float XRotationLimitDegrees => _aimLimits.x;

        public override void Fire()
        {
            var point = _gunPoint.ToRigidTransform();
            _requestsFactory.CreateProjectileRequest(_projectileId, point, PlayerEntity);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (TargetDesignator == null)
                return;
            var data = TargetDesignator.TargetDataProperty.CurrentValue;
            if (data.IsDefined)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(data.Position, 2f);
            }
        }
#endif
    }
}
