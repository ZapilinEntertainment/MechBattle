using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ZE.MechBattle.Vfx
{
    public enum VfxPlayMode : byte { SingleShotAndDestroy, SingleInstanceMultiEmission, PlayingInstancesPool }

    [CreateAssetMenu(fileName = "VfxData", menuName = "Scriptable Objects/VfxData")]
    public class VfxData : ScriptableObject
    {
        [Serializable]
        public struct VfxEffectData
        {
            public VfxPlayMode Mode;
            public ParticleSystem Prefab;
            public int EmitCount;
            public int MaxInstancesCount;
            public float PlayDuration;
        }


        [SerializeField] private SerializedDictionary<string, VfxEffectData> _data;

        public bool TryGetVfxData(string key, out VfxEffectData data) => _data.TryGetValue(key, out data);
    
    }
}
