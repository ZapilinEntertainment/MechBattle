using UnityEngine;

namespace ZE.MechBattle
{
    public class DestructibleDecoration : DisposableGameObject, IView, IDamageableView
    {
        [SerializeField] private DamageableEntityParameters _parameters;
        [SerializeField] private Collider[] _colliders;
        [SerializeField] private string _destroyEffectKey;
        public string ViewDestroyEffectKey => _destroyEffectKey;

        public int[] GetColliderIds()
        {
            var count = _colliders.Length;
            if (count == 1)
                return new int[1] { _colliders[0].GetInstanceID() };

            var ids = new int[count];
            for (var i = 0; i < count; i++)
            {
                ids[i] = _colliders[i].GetInstanceID();
            }
            return ids;
        }

        public DamageableEntityParameters GetParameters() => _parameters;
        public IView GetView() => this;
    }
}
