using System;
using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = "ProjectilesData", menuName = "Scriptable Objects/ProjectilesData")]
    public class ProjectilesData : ScriptableObject
    {
        [Serializable]
        public struct ProjectileData
        {
            public float Speed;
            public string ViewKey;
            public string ExplosionEffectKey;
            public float ExplosionRadius;
            public float Damage;
            public float Lifetime;
        }

        [SerializeField] private SerializedDictionary<string, ProjectileData> _data = new();

        public bool TryGetProjectileData(string key, out ProjectileData data) => _data.TryGetValue(key, out data);
    
    }
}
