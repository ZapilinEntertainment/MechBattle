using TriInspector;
using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "Scriptable Objects/UnitConfig")]
    public class UnitConfig : ScriptableObject, IUnitConfig
    {
        [field:SerializeReference] public IUnitFeatureConfig[] FeatureConfigs { get; private set; }
        [field: SerializeField] public float Health { get; private set; }
        [field:SerializeField] public MovementCollisionAvoidancePriority CollisionAvoidancePriority { get; private set;} = MovementCollisionAvoidancePriority.SmallUnit;
        [field:SerializeField] public string ViewId { get; private set;}
        [PropertySpace]
        [field:SerializeField] public float TargetSearchRadius { get;private set;}
        [field: SerializeField] public float MinTargetDistance { get; private set; }
        [field: SerializeField] public float TargetSearchCooldown { get; private set; } = 3f;
        [PropertySpace]
        [field: SerializeField] public BehaviourKey BehaviourKey { get; private set; }
        [field: SerializeField] public float MoveSpeed { get; private set; }
        [field: SerializeField] public int CapturePower { get; private set; } = 1;
        
    }

    public interface IUnitConfig
    {
        public IUnitFeatureConfig[] FeatureConfigs { get; }
        BehaviourKey BehaviourKey { get;}
        MovementCollisionAvoidancePriority CollisionAvoidancePriority { get;}
        float Health { get; }
        float TargetSearchRadius { get;}
        float MinTargetDistance { get; }
        float MoveSpeed { get;}
        int CapturePower { get; }
        float TargetSearchCooldown { get; }

    }
}
