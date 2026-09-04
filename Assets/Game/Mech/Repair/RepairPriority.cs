using System;

namespace ZE.MechBattle
{
    public readonly struct RepairPriority : IComparable<RepairPriority>
    {
        public readonly RepairableType Type;
        public readonly int Index;

        public RepairPriority(RepairableType type, int index)
        {
            Type = type;
            Index = index;
        }

        public int CompareTo(RepairPriority other)
        {
            int typeComparison = this.Type.CompareTo(other.Type);
            if (typeComparison != 0) return typeComparison;

            return this.Index.CompareTo(other.Index);
        }
    }

    public enum RepairableType : byte
    {
        MechPart, EnergyCell
    }
}
