using UnityEngine;
using ZE.Utils;

namespace ZE.MechBattle.Vfx
{
    public class RayEffectsPool<T> : MonoViewPoolBase<T>, IRayEffectPlayer where T : MonoBehaviour, IDisposableRayEffectView, IPoolableObject<T>
    {
        public RayEffectsPool(T prefab, Transform hostOfPools) : base(prefab, hostOfPools, nameof(RayEffectsPool<T>), 4, 128)
        {
        }

        IDisposableRayEffectView IRayEffectPlayer.GetRayEffect() => Pool.Get();
    }
}
