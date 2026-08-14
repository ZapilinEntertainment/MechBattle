using Unity.Mathematics;
using UnityEngine;

namespace ZE.MechBattle
{
    [CreateAssetMenu(fileName = nameof(MechConfig), menuName = "Scriptable Objects/" + nameof(MechConfig))]
    public class MechConfig : ScriptableObject
    {
        [SerializeField] private float _upperPartRotationSpeedDegrees = 90f;

        public float UpperPartRotationSpeedRadians => math.radians(_upperPartRotationSpeedDegrees);
    
    }
}
