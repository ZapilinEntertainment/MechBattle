using Unity.Mathematics;
using System.Runtime.CompilerServices;
using Unity.Burst;

namespace ZE.MechBattle.Navigation
{
    //completed by Google AI

    // free: 8 bit
    public readonly struct FlowMapCellData
    {
        public readonly int Value;

        private const int DIRECTION_SHIFT = 16;        
        private const int BYTE_MASK = 0xFF;

        public const int DISTANCE_MASK = 0xFFFF;
        public const int INVALID_EXIT_DISTANCE = ushort.MaxValue;
        public const int STRUCTURE_SIZE = sizeof(int);

        public FlowMapCellData(int value) => Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FlowMapCellData(int direction, ushort exitDistance)
        {
            // Direction  16-23, Distance  0-15
            Value = ((direction & BYTE_MASK) << DIRECTION_SHIFT) |
                    (exitDistance & DISTANCE_MASK);

#if UNITY_EDITOR
            if (direction < 0 || direction > 12)
                UnityEngine.Debug.LogError("Wrong direction value: " + direction);
#endif
        }

        public byte Direction => (byte)((Value >> DIRECTION_SHIFT) & BYTE_MASK);
        public int ExitDistance => Value & DISTANCE_MASK;

        [BurstCompile]
        public static FlowMapCellData FormBlockedCell(HexEdge edge, IntTriangularPos tripos, ushort distance)
        {
            return new FlowMapCellData(
                tripos.IsPeak ? (int)edge.ToNeighbourDirectionFromPeak() : (int)edge.ToNeighbourDirectionFromValley(),
                distance);
        }
    }

}
