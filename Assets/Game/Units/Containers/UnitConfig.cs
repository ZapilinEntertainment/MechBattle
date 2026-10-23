using System;
using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "Scriptable Objects/UnitConfig")]
    public class UnitConfig : ScriptableObject, IUnitConfig
    {
        [field: SerializeField] public float Health { get; private set; }
        [field: SerializeField] public float Damage { get; private set; }
        [field:SerializeField] public MovementCollisionAvoidancePriority CollisionAvoidancePriority { get; private set;} = MovementCollisionAvoidancePriority.SmallUnit;
        [field:SerializeField] public string ViewId { get; private set;}
        [field:SerializeField] public float TargetSearchRadius { get;private set;}        
        [field: SerializeField] public float MoveSpeed { get; private set; }
        [field: SerializeField] public float MaxPrecisionAberration { get; private set; }
        [SerializeField] private CaptureBehaviour _captureBehaviour;
        [SerializeField] private WeaponData _weaponData;
        [SerializeField] private BehaviourKey _behaviourKey;

        public bool TryGetWeaponData(out WeaponData weaponData)
        {
            weaponData = _weaponData;
            return weaponData.Config != null;
        }

        public bool TryGetStateBehaviour(out BehaviourKey behaviourKey)
        {
            behaviourKey = _behaviourKey;
            return behaviourKey != BehaviourKey.Undefined;
        }

        public bool TryGetCaptureBehaviour(out CaptureBehaviour captureBehaviour)
        {
            captureBehaviour = _captureBehaviour; 
            return _captureBehaviour.Type != CaptureBehaviourType.Undefined;
        }
    }

    [Serializable]
    public struct WeaponData
    {
        public ProjectileWeaponConfig Config;
        public ViewPartAttachmentProtocol AttachmentProtocol;
    }

    public interface IUnitConfig
    {
        MovementCollisionAvoidancePriority CollisionAvoidancePriority { get;}
        float Health { get; }
        float Damage { get; }
        float TargetSearchRadius { get;}
        float MoveSpeed { get;}
        float MaxPrecisionAberration { get;}

        bool TryGetStateBehaviour(out BehaviourKey behaviourKey);
        bool TryGetWeaponData(out WeaponData weaponData);
        bool TryGetCaptureBehaviour(out CaptureBehaviour captureBehaviour);

    }
}
