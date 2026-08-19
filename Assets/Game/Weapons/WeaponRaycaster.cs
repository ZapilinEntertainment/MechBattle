using Scellecs.Morpeh;
using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle
{
    public class WeaponRaycaster : IWeaponRayCaster
    {
        public float MaxCastDistance => _maxCastDistance;
        private int _frameIndex;
        private Vector3 _start;
        private Vector3 _end;

        private readonly float _maxCastDistance;
        private readonly World _world;
        private readonly Entity _weaponEntity;
        private readonly IDisposableRayEffectView _rayEffect;

        public WeaponRaycaster(World world, Entity weaponEntity, IDisposableRayEffectView rayEffectView, float maxCastDistance)
        {
            _world = world;
            _weaponEntity = weaponEntity;
            _maxCastDistance = maxCastDistance;
            _rayEffect = rayEffectView;
        }

        public float CalculateCurrentDamageCf()
        {
            var distanceCfSq = math.distancesq(_start, _end) / (_maxCastDistance * _maxCastDistance);
            if (distanceCfSq > 1f)
                return 0f;

            return math.sqrt(distanceCfSq);
        }

        public void Dispose()
        {
            _rayEffect.Dispose();
        }

        public void UpdateEndPoints(Vector3 start, Vector3 end, bool hit)
        {
            _start = start;
            _end = end;
            _rayEffect.Start = _start;
            _rayEffect.End = _end;
            _rayEffect.SetEndEffectActivity(hit);
        }

        public void UpdateFrameIndex(int frameIndex) => _frameIndex = frameIndex;

        public bool IsOutdated(int currentFrameIndex) => 
            // note: there is possible to add some lifetime for disappear effects
            _world.IsDisposed(_weaponEntity) ||
            _frameIndex < currentFrameIndex; 
    }
}
