using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "Scriptable Objects/UnitConfig")]
    public class UnitConfig : ScriptableObject, IUnitConfig
    {
        [field:SerializeField] public MovementCollisionAvoidancePriority CollisionAvoidancePriority { get; private set;} = MovementCollisionAvoidancePriority.SmallUnit;
        [field:SerializeField] public string ViewId { get; private set;}
        [field:SerializeField] public float TargetSearchRadius { get;private set;}
        [field: SerializeField] public BehaviourKey BehaviourKey { get; private set; }
        [field: SerializeField] public float MoveSpeed { get; private set; }
        [field: SerializeField] public float MaxPrecisionAberration { get; private set; }
        [SerializeField] private WeaponData _weaponData;

        public bool TryGetWeaponData(out WeaponData weaponData)
        {
            weaponData = _weaponData;
            return weaponData.Config != null;
        }
    }

    [Serializable]
    public struct WeaponData
    {
        public WeaponConfig Config;
        public WeaponAttachmentProtocol AttachmentProtocol;
    }

    public interface IUnitConfig
    {
        BehaviourKey BehaviourKey { get;}
        MovementCollisionAvoidancePriority CollisionAvoidancePriority { get;}
        float TargetSearchRadius { get;}
        float MoveSpeed { get;}
        float MaxPrecisionAberration { get;}

        bool TryGetWeaponData(out WeaponData weaponData);

    }
}
