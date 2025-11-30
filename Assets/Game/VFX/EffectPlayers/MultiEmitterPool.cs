using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

namespace ZE.MechBattle.Vfx
{
    public class MultiEmitterPool : IEffectPlayer
    {
        private class PoolableEmitter : IRestorable
        {
            public readonly ParticleSystem Emitter;
            private readonly Transform _emitterTransform;

            public bool RestoreIfSessionEnds => true;

            private readonly IObjectPool<PoolableEmitter> _pool;

            public PoolableEmitter(ParticleSystem system, IObjectPool<PoolableEmitter> pool)
            {
                Emitter = system;
                _pool = pool;
                _emitterTransform = Emitter.transform;
            }

            public void Restore() => _pool.Release(this);

            public void Play(float3 pos, quaternion rot)
            {
                _emitterTransform.SetPositionAndRotation(pos, rot);
                Emitter.Play();
            }
        }

        private RestorablesList _restorables;

        private readonly float _playingDuration;
        private readonly ParticleSystem _prefab;
        private readonly Transform _objectsHost;
        private readonly ObjectPool<PoolableEmitter> _pool;
        private readonly IDisposable _subscription;
        private readonly AppFlagsManager _appFlags;

        private const string HOST_NAME = "effects_host";

        public MultiEmitterPool(in VfxData.VfxEffectData data, AppFlagsManager appFlags)
        {
            _appFlags = appFlags;
            _subscription = _appFlags.Subscribe<RestorablesList>(OnRestorablesListChanged);

            _objectsHost = new GameObject(HOST_NAME).transform;
            _prefab = data.Prefab;
            _playingDuration = data.PlayDuration;
            _pool = new ObjectPool<PoolableEmitter>(
                createFunc: CreateEmitter,
                actionOnGet: OnEmitterGet,
                defaultCapacity: 4,
                maxSize: data.MaxInstancesCount);
        }


        public void Play(float3 pos, quaternion rot) => _pool.Get().Play(pos, rot);

        public void Dispose()
        {
            _subscription.Dispose();
            _pool.Dispose();
            GameObject.Destroy(_objectsHost.gameObject);
        }

        private void OnRestorablesListChanged(bool isPresented)
        {
            _restorables = isPresented ? _appFlags.GetFirstFlag<RestorablesList>() : null;
        }

        private PoolableEmitter CreateEmitter() 
        {
            var emitter = GameObject.Instantiate(_prefab, _objectsHost);
            return new(emitter, _pool);
        }

        /// <summary>
        /// in session: will be returned by RestorationSystem (end of timer) or RestorablesList(end of session)
        /// not in session: not implemented
        /// </summary>
        private void OnEmitterGet(PoolableEmitter emitter) 
        {
            if (_restorables != null)
            {
                _restorables.RegisterRestorable(emitter, _playingDuration);
            }
            else
            {
                throw new NotImplementedException("non-session restorables not implemented");
            }
        }
            
    }
}
