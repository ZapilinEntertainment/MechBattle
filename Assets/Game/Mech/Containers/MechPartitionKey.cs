using System;

namespace ZE.MechBattle
{
    [Serializable]
    public struct MechPartitionKey
    {
        public MechPartitionType Type;
        public int Index;
        private const char SPLIT_SYMBOL = ':';

        public static MechPartitionKey Center => new() { Type = MechPartitionType.Center, Index = 0 };
        public static MechPartitionKey LeftArm => new() { Type = MechPartitionType.Arm, Index = 0 };
        public static MechPartitionKey RightArm => new() { Type = MechPartitionType.Arm, Index = 1 };
        public static MechPartitionKey LeftLeg => new() { Type = MechPartitionType.Leg, Index = 0 };
        public static MechPartitionKey RightLeg => new() { Type = MechPartitionType.Leg, Index = 1 };

        public static bool TryDecode(string str, out MechPartitionKey key)
        {
            var split = str.Split(SPLIT_SYMBOL);
            key = default;
            if (split.Length != 2)
                return false;

            if (!Enum.TryParse<MechPartitionType>(split[0], ignoreCase: true, out var partitionType)
                || !int.TryParse(split[1], out var index))
                return false;

            key = new() { Type = partitionType, Index = index };
            return true;
        }

        public override readonly string ToString() => Type.ToString() + SPLIT_SYMBOL + Index.ToString();
    }
}
