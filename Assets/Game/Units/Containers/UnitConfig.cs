using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "Scriptable Objects/UnitConfig")]
    public class UnitConfig : ScriptableObject
    {
        [field:SerializeField] public MovementCollisionAvoidancePriority CollisionAvoidancePriority { get; private set;} = MovementCollisionAvoidancePriority.SmallUnit;
        [field:SerializeField] public string ViewKey { get; private set;}
        [field:SerializeField] public float TargetSearchRadius { get;private set;}
        [field: SerializeField] public BehaviourKey BehaviourKey { get; private set; }
        [field: SerializeField] public float MoveSpeed { get; private set; }
    }
}
