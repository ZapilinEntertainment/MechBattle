using System;

namespace ZE.MechBattle
{
    [Serializable]
    public struct MechPartitionConfig
    {
        public MechPartitionKey Key;
        public string RootPartId;
        public ViewPartAttachmentProtocol AttachProtocol;
    
    }
}
