using UnityEngine;
using AYellowpaper.SerializedCollections;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(CollidersConfiguration), menuName = "Scriptable Objects/" + nameof(CollidersConfiguration))]
    public class CollidersConfiguration : ScriptableObject
    {
        [SerializeField] private SerializedDictionary<string, ColliderSetupInfo> _values;

        public bool TryGetColliderSetupInfo(string key, out ColliderSetupInfo setupInfo) => _values.TryGetValue(key, out setupInfo);
    
    }
}
