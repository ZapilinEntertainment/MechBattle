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
        public Chassis(Transform transform, ChassisSettings chassisSettings)
        {
            HipLength = chassisSettings.HipLength;
            AnkleLength= chassisSettings.AnkleLength;
            HipsDistance= chassisSettings.HipsDistance;
            Transform = transform;
        }
    }
}
