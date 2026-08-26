using System;

namespace ZE.MechBattle
{
    [Serializable]
    public struct MechColliderConfig
    {
        public string ViewPartId;
        public MechPartitionKey PartitionKey;
        public ColliderSetupInfo ColliderSetupInfo;    
    }
}
