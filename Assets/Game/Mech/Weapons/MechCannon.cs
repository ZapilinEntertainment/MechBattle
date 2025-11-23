using UnityEngine;

namespace ZE.MechBattle.Weapons
{
    public class MechCannon : MechWeapon
    {
        [SerializeField] private float _aimSpeed = 30f;
        [SerializeField] private Vector2 _aimLimits;


        public override bool ShowInterfaceAim => true;
        public override float AimSpeed => _aimSpeed;
        public override float YRotationLimitDegrees => _aimLimits.y;
        public override float XRotationLimitDegrees => _aimLimits.x;

        public override void Fire()
        {
            
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
