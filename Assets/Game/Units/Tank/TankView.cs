using UnityEngine;

namespace ZE.MechBattle
{
    public class TankView : DisposableGameObject, IDamageableView
    {
        [SerializeField] private Transform _tower;
        [SerializeField] private Transform _trunk;
        [SerializeField] private float _trunkLength = 2f;
        [Space]
        [SerializeField] private string _destroyEffectKey = "tank_destroy";
        [SerializeField] private Collider _collider;
        [SerializeField] private DamageableEntityParameters _damageParameters;
        [Space]
        [SerializeField] private float _speed = 5f;
        [SerializeField] private float _rotationSpeed = 30f;
            

        public string ViewDestroyEffectKey => _destroyEffectKey;

        public int[] GetColliderIds() => new int[1] {_collider.GetInstanceID()};

        public DamageableEntityParameters GetParameters() => _damageParameters;
        public float Speed => _speed;
        public float RotationSpeed => _rotationSpeed;
    }
}
