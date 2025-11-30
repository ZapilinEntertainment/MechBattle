using Unity.Mathematics;
using UnityEngine;
using VContainer;

namespace ZE.MechBattle.Vfx
{
    public class VfxEffectPlayersFactory
    {
        private readonly AppFlagsManager _appFlags;

        [Inject]
        public VfxEffectPlayersFactory(AppFlagsManager appFlags) 
        { 
            _appFlags = appFlags;
        }

        public IEffectPlayer CreateEffect(in VfxData.VfxEffectData data)
        {
            switch (data.Mode)
            {
                case VfxPlayMode.SingleShotAndDestroy: return new SingleShotParticlePlayer(data.Prefab);
                case VfxPlayMode.SingleInstanceMultiEmission: return new SingleInstanceMultiEmissionPlayer(data);
                case VfxPlayMode.PlayingInstancesPool: return new MultiEmitterPool(data, _appFlags);
                default: return null;
            }
        }

    }

    public class SingleShotParticlePlayer : IEffectPlayer
    {
        public readonly ParticleSystem _prefab;

        public SingleShotParticlePlayer(ParticleSystem prefab) => _prefab = prefab;

        public void Play(float3 pos, quaternion rot)
        {
            var obj = GameObject.Instantiate<ParticleSystem>(_prefab, pos, rot);
            obj.Play();
        }

         public void Dispose() { }
    }

    public class SingleInstanceMultiEmissionPlayer : IEffectPlayer
    {
        private readonly int _emitVolume;
        private readonly ParticleSystem _emitter;
        private readonly Transform _emitterTransform;
        public SingleInstanceMultiEmissionPlayer(in VfxData.VfxEffectData data)
        {
            _emitVolume = data.EmitCount;
            _emitter = GameObject.Instantiate(data.Prefab);
            _emitterTransform = _emitter.transform;
        }

        public void Play(float3 pos, quaternion rot)
        {
            _emitterTransform.SetPositionAndRotation(pos, rot);
            _emitter.Emit(_emitVolume);
        }

        public void Dispose()
        {
            GameObject.Destroy(_emitter.gameObject);
        }
    }
}
