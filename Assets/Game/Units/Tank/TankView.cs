using UnityEngine;

namespace ZE.MechBattle
{
    public class TankView : SimpleView, IUnitConfig, IComplexMonoView, ISingleColliderView, IFactionableView
    {
        [SerializeField] private Transform _tower;
        [SerializeField] private Transform _barrel;
        [SerializeField] private UnitConfig _unitConfig;
        [SerializeField] private Collider _collider;
        [SerializeField] private Renderer _renderer;

        public bool TryGetWeaponData(out WeaponData weaponData) => _unitConfig.TryGetWeaponData(out weaponData);

        public bool TryGetPartByKey(ViewPartKey key, out IViewPart viewPart)
        {
            if (key.Type == ViewPartType.Tower)
            {
                viewPart = new ViewPartContainer(_tower);
                return true;
            }

            if (key.Type == ViewPartType.Barrel)
            {
                viewPart = new ViewPartContainer(_barrel);
                return true;
            }

            viewPart = null;
            return false;
        }

        public void ApplyFactionMaterial(Material material)
        {
            _renderer.sharedMaterial = material;
        }

        public float Damage => _unitConfig.Damage;
        public float Health => _unitConfig.Health;
        public BehaviourKey BehaviourKey => _unitConfig.BehaviourKey;
        public MovementCollisionAvoidancePriority CollisionAvoidancePriority => _unitConfig.CollisionAvoidancePriority;
        public float TargetSearchRadius => _unitConfig.TargetSearchRadius;
        public float MoveSpeed => _unitConfig.MoveSpeed;
        public float MaxPrecisionAberration => 0.02f;

        public int ColliderInstanceId => _collider.GetInstanceID();
    }
}
