using UnityEngine;

namespace ZE.MechBattle.Vfx
{
    public class RayEffectsPool<T> : MonoViewPoolBase<T>, IRayEffectPlayer where T : MonoBehaviour, IDisposableRayEffectView, IPoolableObject<T>
    {
        public RayEffectsPool(T prefab, Transform hostOfPools) : base(prefab, hostOfPools)
        {
        }

        protected override string PoolHostObjectName => nameof(RayEffectsPool<T>);

        protected override int DefaultCapacity => 4;

        IDisposableRayEffectView IRayEffectPlayer.GetRayEffect() => Pool.Get();
    }
}
