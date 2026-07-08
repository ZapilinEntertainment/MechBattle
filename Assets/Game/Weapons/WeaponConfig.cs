using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = "WeaponConfig", menuName = "Scriptable Objects/WeaponConfig")]
    public class WeaponConfig : ScriptableObject
    {
        [field:SerializeField] public float MinRange { get; private set;}
        [field: SerializeField, Range(0,1)] public float RecommendedRangePc { get; private set; } = 0.8f;
        [field: SerializeField] public float MaxRange { get; private set; }
        [field: SerializeField] public float Damage { get; private set; }
        [field: SerializeField] public float Cooldown { get; private set; }
        [field: SerializeField] public string ProjectileId { get; private set; }
    }
}
