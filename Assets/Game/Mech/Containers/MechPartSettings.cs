using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace ZE.MechBattle
{
    [Serializable]
    public struct MechPartSettings
    {
        public string Root;
        public MechPartitionKey Partition;
        public ViewPartConstructionProtocol ConstructProtocol;
        public ViewPartAttachmentProtocol AttachProtocol;
        public float RotationSpeedDegrees;
        public ForwardRotationLimits RotationLimits;
        public List<string> SpecialKeywords;

        public float RotationSpeedRadians => math.radians(RotationSpeedDegrees);
    }
}
