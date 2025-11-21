using UnityEngine;

namespace ZE.MechBattle
{
    // reference type- there can be some dynamic parameters (different behavior based on damage for ex.)
    public class Chassis 
    {
        public readonly float HipLength;
        public readonly float AnkleLength;
        public readonly float HipsDistance;
        public readonly Transform Transform;
        public float LegLength => AnkleLength + HipsDistance;

        public Chassis(Transform transform, float hipLength, float ankleLength, float hipsDistance)
        {
            HipLength = hipLength;
            AnkleLength= ankleLength;
            HipsDistance= hipsDistance;
            Transform = transform;
        }
    }
}
